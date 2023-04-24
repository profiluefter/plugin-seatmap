import { Injectable } from '@angular/core';
import {Configuration} from "../backend";
import {AuthService} from "./auth.service";
import {environment} from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService {
  private config: Configuration = new Configuration();

  constructor(private authService: AuthService) {}

  async init(): Promise<void> {
    const token = await this.authService.getToken();
    this.config = new Configuration({
      credentials: {
        'Bearer': token
      },
      basePath: environment.backend
    });
  }

  getConfig(): Configuration {
    return this.config;
  }
}
