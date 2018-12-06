import { PaginatedResult } from './../_models/pagination';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) { }

  getUsers(pageNumber?, pageSize?, userParams?: any): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();
    let urlParameters = new HttpParams();

    if (pageNumber && pageSize) {
      urlParameters = urlParameters.append('pageNumber', pageNumber);
      urlParameters = urlParameters.append('pageSize', pageSize);
    }

    if (userParams) {
      urlParameters = urlParameters.append('gender', userParams.gender);
      urlParameters = urlParameters.append('minAge', userParams.minAge);
      urlParameters = urlParameters.append('maxAge', userParams.maxAge);
      urlParameters = urlParameters.append('orderBy', userParams.orderBy);
    }

    return this.http.get<User[]>(this.baseUrl + 'users', { observe: 'response', params: urlParameters })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          const paginationHeaders = response.headers.get('Pagination');
          if (paginationHeaders) {
            paginatedResult.pagination = JSON.parse(paginationHeaders);
          }
          return paginatedResult;
        })
      );
  }

  getUser(id): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}users/${id}`);
  }

  updateUser(id: number, user: User) {
    return this.http.put(`${this.baseUrl}users/${id}`, user);
  }

  setMainPhoto(userId: number, photoId: number) {
    return this.http.put(`${this.baseUrl}users/${userId}/photos/${photoId}/setMain`, {});
  }

  deletePhoto(photoId: number, userId: number) {
    return this.http.delete(`${this.baseUrl}users/${userId}/photos/${photoId}`);
  }
}
