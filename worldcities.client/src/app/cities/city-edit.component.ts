import { Component, OnInit,OnDestroy } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators, AbstractControl, AsyncValidatorFn } from '@angular/forms';

import { map, takeUntil } from 'rxjs/operators';
import { Observable, Subscription, Subject } from 'rxjs';
import { environment } from './../../environments/environment';
import { BaseFormComponent } from '../base-form.component';

import { City } from './city';
import { Country } from './../countries/country';

import { CityService } from './city.service';

@Component({
  selector: 'app-city-edit',
  templateUrl: './city-edit.component.html',
  styleUrls: ['./city-edit.component.scss']
})
export class CityEditComponent extends BaseFormComponent implements OnInit, OnDestroy {

  // the view title
  title?: string;

  // the form model
  //form!: FormGroup;

  // the city object to edit or create
  city?: City;


  // the city object id, as fetched from the active route:
  // It's NULL when we're adding a new city,
  // and not NULL when we're editing an existing one.
  id?: number;

  // the countries array for the select
 // countries?: Country[];
  // the countries observable for the select (using async pipe)
  countries?: Observable<Country[]>;

  // Activity Log (for debugging purposes)
  activityLog: string = '';

  private subscriptions: Subscription = new Subscription();
  private destroySubject = new Subject();
  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private cityService: CityService) {
    super();
  }

  ngOnInit() {
    this.form = new FormGroup({
      name: new FormControl('', Validators.required),
      lat: new FormControl('', [Validators.required, Validators.pattern(/^[-]?[0-9]+(\.[0-9]{1,4})?$/)]),
      lon: new FormControl('', [Validators.required, Validators.pattern(/^[-]?[0-9]+(\.[0-9]{1,4})?$/)]),
      countryId: new FormControl('', Validators.required)
    }, null, this.isDupeCity());

    // react to form changes
    this.subscriptions.add(this.form.valueChanges
     // .pipe(takeUntil(this.destroySubject))
      .subscribe(() => {
        if (!this.form.dirty) {
          this.log("Form Model has been loaded.");
        }
        else {
          this.log("Form was updated by the user.");
        }
      }));

    // react to changes in the form.name control
    this.subscriptions.add(this.form.get("name")!.valueChanges
     // .pipe(takeUntil(this.destroySubject))
      .subscribe(() => {
        if (!this.form.dirty) {
          this.log("Name has been loaded with initial values.");
        }
        else {
          this.log("Name was updated by the user.");
        }
      }));

    this.loadData();
  }
  ngOnDestroy() {
    this.subscriptions.unsubscribe();
    // emit a value with the takeUntil notifier
   // this.destroySubject.next(true);
    // complete the subject
    //this.destroySubject.complete();
  }
 

  log(str: string) {
    //this.activityLog += "["
    //  + new Date().toLocaleString()
    //  + "] " + str + "<br />";
    console.log("["
      + new Date().toLocaleString()
      + "] " + str);
  }

  loadData() {

    // load countries
    this.loadCountries();

    // retrieve the ID from the 'id' parameter
    var idParam = this.activatedRoute.snapshot.paramMap.get('id');
    this.id = idParam ? +idParam : 0;
    if (this.id) {
      // EDIT MODE

      // fetch the city from the server
      //var url = environment.baseUrl + 'api/Cities/' + this.id;
      //this.http.get<City>(url)
      this.cityService.get(this.id).subscribe({
        next: (result) => {
          this.city = result;
          this.title = "Edit - " + this.city.name;

          // update the form with the city value
          this.form.patchValue(this.city);
        },
        error: (error) => console.error(error)
      });
    }
    else {
      // ADD NEW MODE

      this.title = "Create a new City";
    }
  }

  loadCountries() {
    // fetch all the countries from the server
    //var url = environment.baseUrl + 'api/Countries';
    //var params = new HttpParams()
    //  .set("pageIndex", "0")
    //  .set("pageSize", "9999")
    //  .set("sortColumn", "name");

    //this.http.get<any>(url, { params })
    //.subscribe({
    //  next: (result) => {
    //    this.countries = result.data;
    //  },
    //  error: (error) => console.error(error)
    //});

    this.countries = this.cityService
      .getCountries(
        0,
        9999,
        "name",
        "asc",
        null,
        null)
      .pipe(map(x => x.data));
  }

  onSubmit() {
    var city = (this.id) ? this.city : <City>{};
    if (city) {
      city.name = this.form.controls['name'].value;
      city.lat = +this.form.controls['lat'].value;
      city.lon = +this.form.controls['lon'].value;
      city.countryId = +this.form.controls['countryId'].value;

      if (this.id) {
        // EDIT mode

        //var url = environment.baseUrl + 'api/Cities/' + city.id;
        //this.http
        //  .put<City>(url, city)
        this.cityService.put(city)  
          .subscribe({
            next: (result) => {
              console.log("City " + city!.id + " has been updated.");

              // go back to cities view
              this.router.navigate(['/cities']);
            },
            error: (error) => console.error(error)
          });
      }
      else {
        // ADD NEW mode
        //var url = environment.baseUrl + 'api/Cities';
        //this.http
        //  .post<City>(url, city)
        this.cityService.post(city)
          .subscribe({
            next: (result) => {

              console.log("City " + result.id + " has been created.");

              // go back to cities view
              this.router.navigate(['/cities']);
            },
            error: (error) => console.error(error)
          });
      }
    }
  }

  isDupeCity(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<{ [key: string]: any } | null> => {

      var city = <City>{};
      city.id = (this.id) ? this.id : 0;
      city.name = this.form.controls['name'].value;
      city.lat = +this.form.controls['lat'].value;
      city.lon = +this.form.controls['lon'].value;
      city.countryId = +this.form.controls['countryId'].value;

      //var url = environment.baseUrl + 'api/Cities/IsDupeCity';
      //return this.http.post<boolean>(url, city)
      return this.cityService.isDupeCity(city)
        .pipe(map(result => {
           return (result ? { isDupeCity: true } : null);
      }));
    }



  }
}
