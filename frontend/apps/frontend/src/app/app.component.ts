import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Observable, from, map, of, share, switchMap, tap } from 'rxjs';
import { DatashareService } from './core/datashare.service';
import { SeatWithStudent } from './models/seat-with-student';
import { StudentDecorator } from './models/student-decorator';
import { BASE_PATH, CsvService, FileDto, SeatDto, WinWordDto, WordService } from './swagger';
import { DatePipe } from '@angular/common';
import { saveAs } from 'file-saver';
import { NgxCaptureService } from 'ngx-capture';
import { HttpClient, HttpHeaders } from '@angular/common/http';

const defaultUrl = 'http://localhost:5000';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  private backendUrl = defaultUrl;

  title = 'Seatmap generator';
  clazzNames: string[] = [];
  selectedClazzName = '';
  allStudents: StudentDecorator[] = [];
  studentsOfSelectedClazz = '';
  roomNames: string[] = [];
  selectedRoomName = '';
  seatsOfCurrentRoom: SeatDto[] = [];
  roomCache: { [key: string]: SeatDto[] } = {};
  isFileImportVisible = false;
  isGeneratingWordFile = false;

  constructor(
    @Inject(BASE_PATH) baseUrl: string,
    private csvService: CsvService,
    private wordService: WordService,
    private datashare: DatashareService,
    private datePipe: DatePipe,
    private captureService: NgxCaptureService,
    private http: HttpClient,
  ) {
    if (baseUrl) this.backendUrl = baseUrl;
    console.log(`backendUrl=${this.backendUrl}`);
  }

  ngOnInit(): void {
    // const httpOptions = {
    //   headers: new HttpHeaders({
    //     'Accept': 'application/octet-stream',
    //   }),
    //   responseType: 'blob' as 'json',
    // };
    // const url = 'https://localhost:7226/ReadFile?fileName=quaxi.docx&fullPath=C:/Users/Besitzer/Desktop/quaxi.docx';
    // this.http.get(url, httpOptions)
    //   .subscribe(_ => console.log('xxxxxxxxxxxxxxxxxxxxxx'));
    this.csvService.classesGet().pipe(
      tap(x => this.clazzNames = x.select(x => x.name)),
      tap(_ => console.log('clazzes read')),
      switchMap(_ => this.csvService.studentsGet()),
      tap(x => this.allStudents = x.select(y => new StudentDecorator(y))),
      tap(_ => console.log('students read')),
      switchMap(_ => this.csvService.roomsGet()),
      tap(x => this.roomNames = x.select(x => x.name)),
      tap(_ => console.log('rooms read')),
    )
      .subscribe(_ => {
        this.selectedClazzName = this.clazzNames[Math.floor(Math.random() * this.clazzNames.length)];
        this.onClazzSelected();
        this.useRoom(this.roomNames[Math.floor(Math.random() * this.roomNames.length)]);
      });
  }

  getStudentNamesAsList(): string[] { return this.studentsOfSelectedClazz.split('\n').where(x => x.trim().length > 0); }
  getNrUnblockedSeats(): number { return this.seatsOfCurrentRoom.where(x => !x.isBlocked).length; }
  isEnoughPcs(): boolean { return this.getStudentNamesAsList().length <= this.getNrUnblockedSeats(); }

  onClazzSelected(): void {
    console.log(`onClazzSelected ${this.selectedClazzName}`);
    this.datashare.currentStudents = this.allStudents.where(x => x.clazzName === this.selectedClazzName);
    this.transformCurrentStudentsToModelString();
  }

  private transformCurrentStudentsToModelString(): void {
    this.studentsOfSelectedClazz = this.datashare.currentStudents
      .select(x => x.displayString)
      .join('\n');
  }

  useRoom(roomName: string): Observable<SeatDto[]> {
    console.log(`useRoom ${roomName}`);
    this.selectedRoomName = roomName;
    const observable = this.roomCache[roomName]
      ? of(this.roomCache[roomName]).pipe(tap(x => console.log('found in cache')))
      : this.csvService.seatsGet(roomName).pipe(share(), tap(x => this.roomCache[roomName] = x));
    observable.subscribe(
      x => {
        this.seatsOfCurrentRoom = x;
        this.datashare.seats = x;
      }
    );
    return observable;
  }
  refresh(): void {
    console.log(`refresh ${this.selectedRoomName}`);
    if (this.selectedRoomName === '') return;
    const studentNames = this.getStudentNamesAsList();
    console.log(`refresh for ${studentNames.length} students`);
    this.datashare.currentStudents = this.allStudents
      .where(x => studentNames.indexOf(x.displayString) >= 0)
      .distinctBy(x => x.displayString);
  }

  exportCsv(): void {
    console.log('exportCsv');
    const csvLines = this.datashare.currentStudents
      .where(x => !!this.getSeatOfStudent(x))
      .select(x => `${this.selectedRoomName};${x.displayString};${this.getSeatOfStudent(x).seat.nr}`);
    // csvLines.forEach(x => console.log(x));
    const csvFullText = csvLines.join('\n');
    console.log(csvFullText);
    const fileName = `${this.datePipe.transform(new Date(), 'yyyy-MM-dd')}_Sitzplan_${this.selectedRoomName}_${this.selectedClazzName}.csv`;
    this.saveToDisk(csvFullText, fileName);
  }

  private getSeatOfStudent(student: StudentDecorator): SeatWithStudent {
    return this.datashare.assignedSeats
      .singleOrDefault(x => x.studentName === student.displayString);
  }

  private saveToDisk(csvFullText: string, fileName: string) {
    console.log(`saving to ${fileName}`);
    const blob = new Blob([csvFullText], { type: 'text/plain' });
    saveAs(blob, fileName);
  }

  async onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      const fileName = file.name;
      console.log(`importing file ${fileName}`);
      const contents = await file.text();
      console.log(`contents: ${contents}`);
      this.readSeatMapFromCsv(fileName, contents);
    }
  }

  private readSeatMapFromCsv(fileName: string, contents: string) {
    const regexString = '(?<date>\\d\\d\\d\\d-\\d\\d-\\d\\d)_Sitzplan_(?<room>\\w+)_(?<clazz>\\w+)(?:_?(<rest>\\w+))?\\.csv$';
    const regex = new RegExp(regexString);
    const match = fileName.match(regex);
    if (!regex.test(fileName)) {
      alert(`Filename ${fileName} does not match pattern ${regexString}`);
      return;
    }
    this.isFileImportVisible = false;
    const groups = match?.groups!;
    const roomName = groups['room'];
    const clazzName = groups['clazz'];
    console.log(`${roomName}/${clazzName}`);
    this.selectedClazzName = clazzName;

    //og08;Brunmaier Maximilian;22
    const csvData = contents.split('\n')
      .select(x => x.split(';'))
      .select(x => {
        const obj =
        {
          room: x[0],
          studentName: x[1],
          seatNr: +x[2]
        };
        return obj;
      });
    const studentNames = csvData.select(x => x.studentName).distinct();
    this.datashare.currentStudents = this.allStudents
      .where(x => studentNames.indexOf(x.displayString) >= 0)
      .distinctBy(x => x.displayString);
    this.transformCurrentStudentsToModelString();
    this.useRoom(roomName).subscribe(seatsOfCurrentRoom => {
      const studentNameOfSeat: { [key: number]: string } = csvData.toDictionary(x => x.seatNr, x => x.studentName);
      const occupiedSeatNrs: number[] = Object.keys(studentNameOfSeat).select(x => +x);
      const seatWithStudentsMap: { [key: string]: SeatWithStudent } = {};
      for (let seatNr of occupiedSeatNrs) {
        const seat = seatsOfCurrentRoom.where(y => y.nr === seatNr)[0];
        const student: StudentDecorator = this.datashare.currentStudents.where(x => x.displayString === studentNameOfSeat[seatNr])[0];//don't use first here, because of 'this' pointer
        if (!seat || !student) {
          console.log(`****seat or student not found: ${JSON.stringify(seat)} / ${JSON.stringify(student)}`);
          continue;
        }
        const seatWithStudent = new SeatWithStudent();
        seatWithStudent.seat = seat;
        seatWithStudent.student = student;
        // console.log(`add for seat ${seat.nr}: ${student.displayString}`);
        const key = `${seat.row}_${seat.col}`;
        seatWithStudentsMap[key] = seatWithStudent;
      }
      // console.log(JSON.stringify(seatWithStudentsMap, null, 2));
      this.datashare.seatWithStudentsMap = seatWithStudentsMap;
    });

  }

  @ViewChild('screen', { static: true }) screen: any;
  img = '';
  makeImage(): void {
    this.captureService
      .getImage(this.screen.nativeElement, true)
      .pipe(
        tap(img => this.img = img)
      )
      .subscribe();
  }

  exportWord(): void {
    this.isGeneratingWordFile = true
    const studentPcList = this.datashare.assignedSeats
      .select(x => { return { pcNr: x.seat.nr, studentName: x.studentName }; });
    // console.log(JSON.stringify(studentPcList, null, 2))
    console.log(`creating wordfile for ${studentPcList.length} students`);
    const winWordDto: WinWordDto = {
      imageBase64: '',
      clazzName: this.selectedClazzName,
      roomName: this.selectedRoomName,
      studentPcList: studentPcList
    };
    const httpOptions = {
      headers: new HttpHeaders({
        'Accept': 'application/octet-stream',
      }),
      responseType: 'blob' as 'json',
    };
    let saveFileName = 'xxx.docx';
    this.captureService
      .getImage(this.screen.nativeElement, true)
      .pipe(
        tap(img => winWordDto.imageBase64 = img,),
        // tap(x => console.log(JSON.stringify(x, null, 2))),
        switchMap(_ => this.wordService.createPost(winWordDto)),
        tap((x: FileDto) => console.log(`wordfile ${x.fileName} created on backend`)),
        tap((x: FileDto) => saveFileName = x.fileName),

        // switchMap(x => this.wordService.readFileGet(x.fileName, x.fullPath)) //does not work
        map(x => `${this.backendUrl}/ReadFile?fileName=${x.fileName}&fullPath=${x.fullPath}`),
        switchMap(url => this.http.get(url, httpOptions))
      )
      .subscribe((data: any) => {
        console.log('wordfile downloaded');
        console.log(`saving to ${saveFileName}`);
        this.isGeneratingWordFile = false;
        const blob = new Blob([data], { type: 'application/octet-stream' });
        saveAs(blob, saveFileName);
      });

  }
}
