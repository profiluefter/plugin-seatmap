export * from './csv.service';
import { CsvService } from './csv.service';
export * from './pdf.service';
import { PdfService } from './pdf.service';
export * from './sample.service';
import { SampleService } from './sample.service';
export * from './values.service';
import { ValuesService } from './values.service';
export const APIS = [CsvService, PdfService, SampleService, ValuesService];
