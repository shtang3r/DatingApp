import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {map} from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  userIsLoggedIn: boolean;

  constructor(private httpClient: HttpClient) { }
  baseUrl = 'http://localhost:5000/api/auth/';

  login(model: any) {
    return this.httpClient.post(this.baseUrl + 'login', model)
    .pipe(map((response:any) => {
        const user = response;
        if (user) {
          localStorage.setItem('token', user.token);
        }
      })
    );
  }

  register(model: any) {
    return this.httpClient.post(this.baseUrl + 'register', model);
  }

}
