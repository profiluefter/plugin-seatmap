import { Injectable } from '@angular/core';
import { JwtDecoderService } from "./jwt-decoder.service";
import {environment} from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private jwtDecoder: JwtDecoderService) { }

  async getToken(): Promise<string> {
    let accessToken = await this.jwtDecoder.getJwt();
    if (accessToken === undefined || !environment.production)
      accessToken = environment.devToken;
    return `Bearer ${accessToken}`;
  }
}
