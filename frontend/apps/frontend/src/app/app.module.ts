import {APP_INITIALIZER, NgModule} from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { DatePipe } from '@angular/common';

import { AppComponent } from './app.component';
//import { ApiModule, BASE_PATH } from './swagger';
import { environment } from '../environments/environment';
import { SeatDisplayComponent } from './seat-display/seat-display.component';
import { CoreModule } from './core/core.module';
import {ConfigurationService} from "./core/configuration.service";
import {ApiModule, Configuration} from './backend';

export function initConfig(configService: ConfigurationService): () => Promise<void> {
  return async () => {
    await configService.init();
  };
}

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
   // { provide: BASE_PATH, useValue: environment.backend },
    DatePipe,
    {
      provide: APP_INITIALIZER,
      useFactory: initConfig,
      deps: [ConfigurationService],
      multi: true
    },
    {
      provide: Configuration,
      useFactory: (configService: ConfigurationService) => configService.getConfig(),
      deps: [ConfigurationService],
      multi: false
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
