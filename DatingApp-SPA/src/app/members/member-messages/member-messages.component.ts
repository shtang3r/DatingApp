import { AlertifyService } from './../../_services/alertify.service';
import { UserService } from './../../_services/user.service';
import { AuthService } from './../../_services/auth.service';
import { Component, OnInit, Input } from '@angular/core';
import { User } from 'src/app/_models/user';
import { Message } from 'src/app/_models/message';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() recepientId: number;
  messages: Message[];
  newMessage: any = {};

  constructor(private authService: AuthService,
              private userService: UserService,
              private alertifyService: AlertifyService) { }

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    const currentUserId = +this.authService.getCurrentUserId();
    this.userService.getMessageThread(currentUserId, this.recepientId)
        .pipe( tap(messages => {
          for (let i = 0; i < messages.length; i++) {
            if (!messages[i].isRead && messages[i].recepientId === currentUserId) {
              this.userService.markAsRead(currentUserId, messages[i].id);
            }
          }
        }))
        .subscribe(messages => {
          this.messages = messages;
        }, error => {this.alertifyService.error(error); });
  }

  sendMessage() {
    const currentUserId = this.authService.getCurrentUserId();
    this.newMessage.recepientId = this.recepientId;
    this.userService.sendMessage(currentUserId, this.newMessage)
      .subscribe((message: Message) => {
        // message.senderPhotoUrl = this.authService.currentPhotoUrl;
        this.alertifyService.success('sucessfully sent a message');
        this.messages.unshift(message);
        this.newMessage.content = '';
      }, error => { this.alertifyService.error(error); });
  }

}
