import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  // 1. Kural: Her isteğe withCredentials ekle (Kasadaki Refresh Token gitsin diye)
  let clonedReq = req.clone({
    withCredentials: true
  });

  // 2. Kural: Eğer RAM'de JWT varsa onu da ekle
  if (token) {
    clonedReq = clonedReq.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  // 3. Kural: İsteği gönder ve dönen cevabı dinle (401 Hatası gelirse yakala)
  return next(clonedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Eğer hata 401 ise (Yetkisiz) ve şu an yenileme yapmıyorsak
      if (error.status === 401 && !isRefreshing) {
        isRefreshing = true;

        // Arka plandan yeni JWT iste (Cookie zaten otomatik gidecek)
        return authService.refreshToken().pipe(
          switchMap((res: any) => {
            isRefreshing = false;
            // Yeni bilet geldi! Eski başarısız olan isteği (req) YENİ biletle tekrar yolla!
            const newReq = req.clone({
              withCredentials: true,
              setHeaders: {
                Authorization: `Bearer ${res.token}`
              }
            });
            return next(newReq);
          }),
          catchError((err) => {
            isRefreshing = false;
            // Eğer Refresh Token da eskidiyse (30 gün dolduysa), sistemi kilitle ve login'e at.
            authService.logout();
            return throwError(() => err);
          })
        );
      }

      return throwError(() => error);
    })
  );
};
