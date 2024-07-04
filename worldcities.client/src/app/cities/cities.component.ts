import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from './../../environments/environment';

import { City } from './city';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-cities',
  templateUrl: './cities.component.html',
  styleUrls: ['./cities.component.scss']
})
export class CitiesComponent implements OnInit {
  public cities!: City[];
 // citiesObs!: Observable<City[]>

  constructor(private http: HttpClient) {
  }

  ngOnInit() {
    this.http.get<City[]>(environment.baseUrl + 'api/Cities')
      .subscribe({
        next: (result) => {
          this.cities = result;
        },
        error: (error:Error) => console.error(error)
      });
    //this.citiesObs = this.http.get<City[]>(environment.baseUrl + 'api/Cities');
  }
}
