import { UserService } from './../_services/user.service';
import { AlertifyService } from './../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Component, OnInit, Input } from '@angular/core';
import { User } from '../_models/user';
import { Pagination, PaginatedResult } from '../_models/pagination';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
@Input() users: User[];
  pagination: Pagination;
  likesParam: string;
  constructor(private route: ActivatedRoute,
              private alertifyService: AlertifyService,
              private userService: UserService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data['users'].result;
      this.pagination = data['users'].pagination;
    });
    this.likesParam = 'Likers';
  }

  loadUsers() {
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, null, this.likesParam)
      .subscribe((paginatedResult: PaginatedResult<User[]>) => {
        this.users = paginatedResult.result;
        this.pagination = paginatedResult.pagination;
      }, error => {this.alertifyService.error(error); });
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }
}
