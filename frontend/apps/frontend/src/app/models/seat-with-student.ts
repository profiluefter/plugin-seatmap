import { SeatDto } from "../backend";
import { StudentDecorator } from "../models/student-decorator";

export class SeatWithStudent {
  seat: SeatDto = null!;
  student: StudentDecorator | null = null;

  get title(): string {
    const pad = this.seat.nr < 10 ? '0' : '';
    return `PC ${pad}${this.seat.nr}`;
  }
  get studentName(): string { return this.student ? this.student!.displayString : ''; }
  get imagePath(): string { return this.student ? this.student!.imagePath : ''; }
  get isBlocked(): boolean { return this.seat.isBlocked; }
  set isBlocked(val: boolean) { this.seat.isBlocked = val; }
  get hasStudent(): boolean { return !!this.student; }

  getCssClasses(): string[] {
    const css: string[] = [];
    css.push('seat');
    if (this.isBlocked) css.push('brushPcBlocked');
    if (!this.hasStudent) css.push('brushPcEmpty');
    css.push(this.seat.isToFront ? 'isSeatToFront' : 'isSeatToBack');
    return css;
  }
}
