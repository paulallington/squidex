/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';

import { UserDto, UserManagementService } from './../services/users.service';

@Injectable()
export class ResolveUserGuard implements Resolve<UserDto> {
    constructor(
        private readonly userManagementService: UserManagementService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<UserDto> {
        const userId = this.findParameter(route, 'userId');

        if (!userId) {
            throw 'Route must contain user id.';
        }

        const result =
            this.userManagementService.getUser(userId).toPromise()
                .then(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);

                        return null;
                    }

                    return dto;
                }).catch(() => {
                    this.router.navigate(['/404']);

                    return null;
                });

        return result;
    }

    private findParameter(route: ActivatedRouteSnapshot, name: string): string | null {
        let result: string | null = null;

        while (route) {
            result = route.params[name];

            if (result || !route.parent) {
                break;
            }

            route = route.parent;
        }

        return result;
    }
}