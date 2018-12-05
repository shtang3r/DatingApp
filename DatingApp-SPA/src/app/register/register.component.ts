import { AlertifyService } from './../_services/alertify.service';
import { AuthService } from './../_services/auth.service';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormControl, Validators, ValidatorFn, AbstractControl, ValidationErrors, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';
import { Route, Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  user: User;
  registerForm: FormGroup;
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(private authService: AuthService,
    private alertifyService: AlertifyService,
    private fb: FormBuilder,
    private router: Router) { }

  ngOnInit() {
    // this.registerForm = new FormGroup({
    //   username: new FormControl('', Validators.required),
    //   password: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(32)]),
    //   confirmPassword: new FormControl('', [Validators.required])
    // }, this.passwordMatchValidator);
    this.bsConfig = {
      containerClass: 'theme-red'
    };
    this.createRegisterForm();
  }

  register() {
    if (this.registerForm.valid) {
      this.user = Object.assign({}, this.registerForm.value);
      this.authService.register(this.user).subscribe(() => {
        this.alertifyService.success('successfuly registered');
      }, error => this.alertifyService.error(error),
        () => {
          this.authService.login(this.user).subscribe(() => {
            this.router.navigate(['/members']);
          });
        });
      console.log(this.registerForm.value);
      console.log(this.registerForm.errors);
    }
  }

  createRegisterForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(32)]],
      confirmPassword: ['', [Validators.required]]
    }, { validator: this.passwordMatchValidator });
  }

  passwordMatchValidator(formGroup: FormGroup) {
    return formGroup.get('password').value === formGroup.get('confirmPassword').value ? null : { 'mismatch': true };
  }

  cancel() {
    this.cancelRegister.emit(false);
    this.alertifyService.message('cancelled');
  }
}
