import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { DatePipe } from '@angular/common';

import { AppComponent } from './app.component';
import { ApiModule, BASE_PATH } from './swagger';
import { environment } from '../environments/environment';
import { SeatDisplayComponent } from './seat-display/seat-display.component';
import { CoreModule } from './core/core.module';

@NgModule({
  declarations: [
    AppComponent,
    SeatDisplayComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    ApiModule,
    CoreModule,
  ],
  // use environment.backend as reference to the backend URL
  providers: [
    { provide: BASE_PATH, useValue: environment.backend },
    DatePipe,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
