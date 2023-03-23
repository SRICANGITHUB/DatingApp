import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit{

  model: any ={};
  loggedIn = false;
  //urrentUser$: Observable<User | null> = of(null);

  constructor(public accountservice: AccountService){}

  ngOnInit(): void{
    //this.currentUser$ = this.accountservice.currentUser$;
  }

 

  login() 
  {
    this.accountservice.login(this.model).subscribe({
      next: response => {
        console.log(response);
        this.loggedIn = true;
      },

      error: error=> console.log(error)
    });
    
  }


  logout()
  {
    this.accountservice.logout();
    this.loggedIn=false;
  }

}
