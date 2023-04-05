import { Component, OnInit } from '@angular/core';
import { DatashareService } from '../core/datashare.service';
import { SeatDto } from '../swagger';
import { Position } from './position';
import { SeatWithStudent } from '../models/seat-with-student';

@Component({
  selector: 'app-seat-display',
  templateUrl: './seat-display.component.html',
  styleUrls: ['./seat-display.component.scss']
})
export class SeatDisplayComponent implements OnInit {
  private dragPosition: Position | null = null;
  nrRows = 0;
  nrCols = 0;
  _rowHeight = 0;
  _seatWithStudentsMap: Record<string, SeatWithStudent> = {};

  constructor(
    private datashare: DatashareService,
  ) { }

  ngOnInit(): void {
    this.datashare.shouldDraw().subscribe(x => this.drawSeatMap(true));
    this.datashare.seatMapChanged().subscribe(x => {
      this.drawSeatMap(false);
      for (let key of Object.keys(x)) {
        this._seatWithStudentsMap[key] = x[key];
      }
    });
  }

  private getKey(seatDto: SeatDto): string { return this.buildKey(seatDto.row, seatDto.col); }
  private buildKey(row: number, col: number): string { return `${row}_${col}`; }
  private getSeatAt(row: number, col: number): SeatWithStudent { return this._seatWithStudentsMap[this.buildKey(row, col)]; }

  get rows() { return [...Array(this.nrRows).keys()].select(x => this.nrRows - x); }
  get cols() { return [...Array(this.nrCols).keys()].select(x => this.nrCols - x); }

  drawSeatMap(doAssignSeatings: boolean): void {
    console.log(`SeatDisplayComponent.drawSeatMap - doAssignSeatings=${doAssignSeatings}`);
    const seats = this.datashare.seats;
    this.nrRows = seats.max(x => x.row);
    this.nrCols = seats.max(x => x.col);
    this._seatWithStudentsMap = {};
    seats.forEach(seat => this.createEmptySeat(seat));
    if (doAssignSeatings) this.assignSeatings();
  }

  getTitle(row: number, col: number): string {
    const seatWithStudent = this.getSeatAt(row, col);
    return seatWithStudent ? seatWithStudent.title : '';
  }

  getStudentName(row: number, col: number): string {
    const seatWithStudent = this.getSeatAt(row, col);
    return seatWithStudent ? seatWithStudent.studentName : '';
  }
  getCss(row: number, col: number): string[] {
    const seatWithStudent = this.getSeatAt(row, col);
    return seatWithStudent ? seatWithStudent.getCssClasses() : [];
  }
  getImagePath(row: number, col: number): string {
    const seatWithStudent = this.getSeatAt(row, col);
    return seatWithStudent ? seatWithStudent.imagePath : '';
  }

  toggleIsBlocked(row: number, col: number): void {
    const seatWithStudent = this.getSeatAt(row, col);
    if (!seatWithStudent) return;
    seatWithStudent.isBlocked = !seatWithStudent.isBlocked;
    console.log(`${seatWithStudent.title} --> ${seatWithStudent.isBlocked}`);
    this.drawSeatMap(true);
  }
  createEmptySeat(seat: SeatDto): void {
    const key = this.getKey(seat);
    const seatWithStudent: SeatWithStudent = new SeatWithStudent();
    seatWithStudent.seat = seat;
    this._seatWithStudentsMap[key] = seatWithStudent;
  }

  assignSeatings(): void {
    console.log(`assignSeatings for ${this.datashare.currentStudents.length} students`);
    let availableSeats = Object.values(this._seatWithStudentsMap).where(x => !x.isBlocked);
    availableSeats = [...availableSeats]; //create copy, because below items will be removed
    if (this.datashare.currentStudents.length > availableSeats.length) {
      console.log(`Zuviele Schüler (${this.datashare.currentStudents.length} für Raum (${availableSeats.length} PCs)`)
      return;
    }
    this.datashare.assignedSeats = [];
    for (let student of this.datashare.currentStudents) {
      const idx = Math.floor(Math.random() * availableSeats.length);
      var seat = availableSeats[idx];
      availableSeats.splice(idx, 1);
      seat.student = student;
      this.datashare.assignedSeats.push(seat);
    }
    this.freezeAssignedSeatsInDatashare();
  }
  private freezeAssignedSeatsInDatashare(): void {
    this.datashare.assignedSeats = Object.values(this._seatWithStudentsMap).where(x => x.hasStudent);
  }

  startDrag(row: number, col: number): void {
    // console.log(`startDrag from ${row}/${col}`);
    this.dragPosition = { row: row, col: col };
  }

  drop(row: number, col: number): void {
    // console.log(`drop at ${row}/${col}`);
    if (!this.dragPosition) return;
    const seatFrom = this.getSeatAt(this.dragPosition!.row, this.dragPosition!.col);
    this.dragPosition = null;
    const seatTo = this.getSeatAt(row, col);
    if (seatTo.isBlocked) return;
    const draggedStudent = seatFrom.student;
    seatFrom.student = seatTo.hasStudent ? seatTo.student : null;
    seatTo.student = draggedStudent;
    this.freezeAssignedSeatsInDatashare();
  }
}
