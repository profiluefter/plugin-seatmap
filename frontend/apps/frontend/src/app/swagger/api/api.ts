export * from './csv.service';
import { CsvService } from './csv.service';
export * from './values.service';
import { ValuesService } from './values.service';
export * from './word.service';
import { WordService } from './word.service';
export const APIS = [CsvService, ValuesService, WordService];
