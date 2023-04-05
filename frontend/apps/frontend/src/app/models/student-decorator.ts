import { StudentDto } from "../swagger";

export class StudentDecorator implements StudentDto {
  private _studentDto: StudentDto = null!;

  get lastname() { return this._studentDto.lastname; }
  get firstname() { return this._studentDto.firstname; }
  get clazzName() { return this._studentDto.clazzName; }
  get postalCode() { return this._studentDto.postalCode; }
  get city() { return this._studentDto.city; }

  constructor(studentDto: StudentDto) { this._studentDto = studentDto; }

  get displayString(): string { return `${this.lastname} ${this.firstname}`; }

  private get name(): string { return `${this.lastname}_${this.firstname}`; }
  private get imageName(): string { return `${this.name}.jpg`; }
  get imagePath(): string { return `assets/img/${this.clazzName}/${this.imageName}` };
}
