import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
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

  constructor(public accountservice: AccountService, private router: Router, private toastr:ToastrService){}

  ngOnInit(): void{
    //this.currentUser$ = this.accountservice.currentUser$;
  }

 

  login() 
  {
    this.accountservice.login(this.model).subscribe({
      next: _ => this.router.navigateByUrl('/members')    
      //error: error=> this.toastr.error(error.error)
    });
    
  }


  logout()
  {
    this.accountservice.logout();
    this.router.navigateByUrl('/');
  }

}
