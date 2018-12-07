import { PaginatedResult } from './../_models/pagination';
import { AlertifyService } from './../_services/alertify.service';
import { UserService } from './../_services/user.service';
import { ActivatedRoute } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { AuthService } from '../_services/auth.service';
import { Pagination } from '../_models/pagination';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  messageContainer = 'Unread';

  constructor(private route: ActivatedRoute,
              private authService: AuthService,
              private userService: UserService,
              private alertifyService: AlertifyService) { }

  ngOnInit() {
    this.route.data.subscribe(data=>{
      this.messages = data['messages'].result;
      this.pagination = data['messages'].pagination;
    });
  }

  loadMessages() {
    this.userService.getMessages(this.authService.getCurrentUserId(),
                                 this.pagination.currentPage,
                                 this.pagination.itemsPerPage,
                                 this.messageContainer)
      .subscribe((response: PaginatedResult<Message[]>) => {
        this.messages = response.result;
        this.pagination = response.pagination;
      }, error => { this.alertifyService.error(error); });
  }

  pageChanged(event: any) {
    this.pagination.currentPage = event.page;
    this.loadMessages();
  }

  deleteMessage(id: number) {
    this.alertifyService.confirm('Are you sure you want to delete this message?', () => {
      const currentUserId = this.authService.getCurrentUserId();
      this.userService.deleteMessage(currentUserId, id)
        .subscribe( () => {
          const indexOfMessage = this.messages.findIndex(m => m.id === id);
          this.messages.splice(indexOfMessage, 1);
          this.alertifyService.success('successfully deleted the message');
        }, error => {this.alertifyService.error('failed to delete the message') });
    });
    }
}
