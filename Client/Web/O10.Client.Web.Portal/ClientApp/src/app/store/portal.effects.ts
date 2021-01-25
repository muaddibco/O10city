import { Injectable } from '@angular/core';
import { Actions, Effect, ofType } from '@ngrx/effects';
import * as PortalActions from './portal.actions'

@Injectable()
export class PortalEffects {

  //@Effect()
  //load = this.actions$.pipe(
  //  ofType(PermissionsActions.LOAD_STARTED),
  //  switchMap(loadAction => this.permissionsEditorService.getCIFCResourcePermissions(
  //    loadAction.payload.dirId, loadAction.payload.filerId)),
  //  map((trustees) => {
  //    return {
  //      type: PermissionsActions.LOAD_COMPLETED,
  //      payload: trustees
  //    };
  //  })
  //);

  //@Effect()
  //save = this.actions$.pipe(
  //  ofType(PermissionsActions.PERMISSIONS_EDITING_SAVE_STARTED),
  //  switchMap(saveAction => this.permissionsEditorService.saveCifsResourcePermissions(
  //    saveAction.payload.dirId, saveAction.payload.filerId, saveAction.payload.trustees)),
  //  map((trustees) => {
  //    return {
  //      type: PermissionsActions.PERMISSIONS_EDITING_SAVE_COMPLETED,
  //      payload: trustees
  //    };
  //  })
  //);


  constructor(private actions$: Actions<PortalActions.PortalActions>) { }
}
