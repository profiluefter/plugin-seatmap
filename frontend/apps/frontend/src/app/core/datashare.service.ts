import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { SeatWithStudent } from '../models/seat-with-student';
import { StudentDecorator } from '../models/student-decorator';
import { SeatDto } from '../backend';

@Injectable({
  providedIn: 'root'
})
export class DatashareService {
  private drawSubject = new Subject<string>();
  private mapSubject = new Subject<{ [key: string]: SeatWithStudent }>();
  private _currentStudents: StudentDecorator[] = [];
  private _seats: SeatDto[] = [];
  private _assignedSeats: SeatWithStudent[] = [];
  private _seatWithStudentsMap: { [key: string]: SeatWithStudent } = {};

  constructor() { }

  get currentStudents(): StudentDecorator[] { return this._currentStudents; }
  set currentStudents(students: StudentDecorator[]) {
    console.log(`DatashareService setting ${students.length} students`);
    // students.forEach(x => console.log(`${x.lastname} ${x.firstname}`));
    this._currentStudents = students;
    this.forceRedraw();
  }

  get seats(): SeatDto[] { return this._seats; }
  set seats(seats: SeatDto[]) {
    console.log(`DatashareService setting ${seats.length} seats`);
    this._seats = seats;
    this.forceRedraw();
  }

  get assignedSeats(): SeatWithStudent[] { return this._assignedSeats; }
  set assignedSeats(val: SeatWithStudent[]) { this._assignedSeats = val; }

  private forceRedraw(): void {
    console.log('DatashareService.forceRedraw');
    this.drawSubject.next('');
  }

  get seatWithStudentsMap(): { [key: string]: SeatWithStudent } { return this._seatWithStudentsMap; }
  set seatWithStudentsMap(seatMap: { [key: string]: SeatWithStudent }) {
    console.log('DatashareService setting seatWithStudentsMap');
    this._seatWithStudentsMap = seatMap;
    this.mapSubject.next(this._seatWithStudentsMap);
  }

  shouldDraw(): Observable<string> { return this.drawSubject.asObservable(); }
  seatMapChanged(): Observable<{ [key: string]: SeatWithStudent }> { return this.mapSubject.asObservable(); }

}
