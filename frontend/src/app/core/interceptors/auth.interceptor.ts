import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';


export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('jwt_token');

  // If there is a token, clone the request and add the Authorization header
  if (token) {
    const clonedReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(clonedReq);
  }

  // Otherwise proceed without token
  return next(req);
};
