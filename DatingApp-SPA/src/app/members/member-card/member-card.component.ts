import { AlertifyService } from './../../_services/alertify.service';
import { UserService } from './../../_services/user.service';
import { AuthService } from './../../_services/auth.service';
import { Component, OnInit, Input } from '@angular/core';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  @Input() user: User;

  constructor(private authService: AuthService,
              private userService: UserService,
              private alertifyService: AlertifyService) { }

  ngOnInit() {
  }

  likeUser(recepientId: number) {
    const currentUserId = this.authService.decodedToken.nameid;
    this.userService.sendLike(currentUserId,recepientId).subscribe((response) => {
      console.log('sucessfully liked');
    }, error => { this.alertifyService.error(error); });
  }
}
