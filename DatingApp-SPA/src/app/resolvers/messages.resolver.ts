import { AuthService } from 'src/app/_services/auth.service';
import { Observable, of } from 'rxjs';
import { AlertifyService } from '../_services/alertify.service';
import { UserService } from '../_services/user.service';
import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { Message } from '../_models/message';

@Injectable()
export class MessagesResolver implements Resolve<Message[]> {
    pageNumber = 1;
    pageSize = 10;
    messageContainer = 'Unread';
    constructor(private userService: UserService,
        private router: Router,
        private alertifyService: AlertifyService,
        private authService: AuthService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<Message[]> {
        const currentUserId = this.authService.decodedToken.nameid;
        return this.userService.getMessages(currentUserId, this.pageNumber, this.pageSize, this.messageContainer)
            .pipe(catchError(error => {
                this.alertifyService.error('Problem retrieving messages');
                this.router.navigate(['/home']);
                return of(null);
            })
            );

    }
}
