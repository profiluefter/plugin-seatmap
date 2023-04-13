import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { Observable, from, map, of, share, switchMap, tap } from 'rxjs';
import { DatashareService } from './core/datashare.service';
import { SeatWithStudent } from './models/seat-with-student';
import { StudentDecorator } from './models/student-decorator';
import { BASE_PATH, CsvService, FileDto, PdfService, SeatDto, FileGenerationDataDto } from './swagger';
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
  roomCache: Record<string, SeatDto[]> = {};
  isFileImportVisible = false;
  isGeneratingWordFile = false;

  constructor(
    @Inject(BASE_PATH) baseUrl: string,
    private csvService: CsvService,
    private pdfService: PdfService,
    private datashare: DatashareService,
    private datePipe: DatePipe,
    private captureService: NgxCaptureService,
    private http: HttpClient,
  ) {
    if (baseUrl) this.backendUrl = baseUrl;
    console.log(`backendUrl=${this.backendUrl}`);
  }

  ngOnInit(): void {
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

  private shareStudentsByName(studentNames: string[]): void {
    //Note: this function is required to select students with duplicate names in different classes (e.g. Sarah Mayr)
    //      to select the correct one
    // - first read only students of selected class
    // - if names are found, that are not in this class add them
    const studentsSelectedNames = this.allStudents.where(x => studentNames.indexOf(x.displayString) >= 0);
    const studentsSelectedOfCurrentClass = studentsSelectedNames.where(x => x.clazzName == this.selectedClazzName);
    const studentNamesOfOtherClass = studentNames
      .where(x => studentsSelectedOfCurrentClass.select(x => x.displayString).indexOf(x) < 0);
    const studentsSelectedOfOtherClass = this.allStudents
      .where(x => studentNames.indexOf(x.displayString) >= 0
        && studentsSelectedOfCurrentClass.select(y => y.displayString).indexOf(x.displayString) < 0
        && x.clazzName !== this.selectedClazzName);
    if (studentNamesOfOtherClass.length !== 0) {
      console.log('******************** students of another class found');
      for (const student of studentsSelectedOfOtherClass) {
        console.log(`  adding: ${student.displayString} of class ${student.clazzName}`);
        studentsSelectedOfCurrentClass.push(student);
      }
    }
    this.datashare.currentStudents = studentsSelectedOfCurrentClass;
  }

  refresh(): void {
    console.log(`refresh ${this.selectedRoomName}`);
    if (this.selectedRoomName === '') return;
    const studentNames = this.getStudentNamesAsList();
    console.log(`refresh for ${studentNames.length} students`);
    this.shareStudentsByName(studentNames);
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
    this.shareStudentsByName(studentNames);
    this.transformCurrentStudentsToModelString();
    this.useRoom(roomName).subscribe(seatsOfCurrentRoom => {
      const studentNameOfSeat: Record<number, string> = csvData.toDictionary(x => x.seatNr, x => x.studentName);
      const occupiedSeatNrs: number[] = Object.keys(studentNameOfSeat).select(x => +x);
      const seatWithStudentsMap: Record<string, SeatWithStudent> = {};
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

  exportPdf(): void {
    this.isGeneratingWordFile = true
    const studentPcList = this.datashare.assignedSeats
      .select(x => { return { pcNr: x.seat.nr, studentName: x.studentName }; });
    // console.log(JSON.stringify(studentPcList, null, 2))
    console.log(`creating wordfile for ${studentPcList.length} students`);
    const fileGenerationDataDto: FileGenerationDataDto = {
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
        // tap(img => console.log(img)),
        tap(img => fileGenerationDataDto.imageBase64 = img,),
        switchMap(_ => this.pdfService.pdfCreatePost(fileGenerationDataDto)),
        tap((x: FileDto) => console.log(`file ${x.fileName} created on backend`)),
        tap((x: FileDto) => saveFileName = x.fileName),

        // switchMap(x => this.pdfService.readFileGet(x.fileName, x.fullPath)) //does not work
        map(x => `${this.backendUrl}/pdf/ReadFile?fileName=${x.fileName}&fullPath=${x.fullPath}`),
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
