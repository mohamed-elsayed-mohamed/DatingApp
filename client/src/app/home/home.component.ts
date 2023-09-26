import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  users: any;
  registerMode = false;

  constructor() {}

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  ngOnInit(): void {}

  cancelRegisterMode(event: boolean) {
    this.registerMode = event;
  }
}
