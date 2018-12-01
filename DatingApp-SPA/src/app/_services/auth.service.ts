import { JwtHelperService } from '@auth0/angular-jwt';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {map} from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  userIsLoggedIn: boolean;
  jwtHelper = new JwtHelperService();
  decodedToken: any;

  constructor(private httpClient: HttpClient) { }
  baseUrl = 'http://localhost:5000/api/auth/';

  login(model: any) {
    return this.httpClient.post(this.baseUrl + 'login', model)
    .pipe(map((response: any) => {
        const user = response;
        if (user) {
          localStorage.setItem('token', user.token);
          this.decodedToken = this.jwtHelper.decodeToken(user.token);
          console.log(this.decodedToken);
        }
      })
    );
  }

  register(model: any) {
    return this.httpClient.post(this.baseUrl + 'register', model);
  }

  isLoggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }

  logout() {
    localStorage.removeItem('token');
  }

  getUserName() {
   return this.decodedToken.unique_name;
  }

}
