import { HttpErrorResponse, HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';

export const apiResponseInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (event instanceof HttpResponse && event.body !== null && event.body !== undefined) {
        const body = event.body as any;
        if (body && typeof body === 'object' && 'succes' in body) {
          if (body.succes === false) {
            const errMsg = body.erreurs?.[0] ?? body.message ?? 'Une erreur est survenue.';
            throw new HttpErrorResponse({
              error: { erreurs: body.erreurs ?? [], message: body.message },
              status: event.status,
              statusText: errMsg,
              url: event.url ?? undefined,
            });
          }
          return event.clone({ body: body.donnees });
        }
      }
      return event;
    })
  );
};
