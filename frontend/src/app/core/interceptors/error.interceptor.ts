import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 400 && error.error) {
        // Backend'in gönderdiği form doğrulama hatalarını (validationErrors) yakala
        if (error.error.validationErrors) {
          console.error('Doğrulama Hataları:', error.error.validationErrors);
          
          // İleride buraya Toastr veya SnackBar servisi eklenebilir
          // Örn: toastService.error('Lütfen formdaki hataları düzeltin');
        } else if (error.error.errors) {
          console.error('İşlem Hataları:', error.error.errors);
        }
        
        console.error('Backend Mesajı:', error.error.message);
      } else if (error.status === 401) {
        console.error('Oturum süresi doldu veya yetkisiz erişim.');
        // Örn: router.navigate(['/login']);
      }
      
      // Hatayı component'in de görebilmesi için fırlatmaya devam et
      return throwError(() => error);
    })
  );
};
