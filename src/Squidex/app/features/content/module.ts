/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    CanDeactivateGuard,
    HistoryComponent,
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    ResolvePublishedSchemaGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    ContentFieldComponent,
    ContentPageComponent,
    ContentItemComponent,
    ContentsPageComponent,
    SchemasPageComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: SchemasPageComponent,
        children: [
            {
                path: ''
            },
            {
                path: ':schemaName',
                component: ContentsPageComponent,
                resolve: {
                    schema: ResolvePublishedSchemaGuard, appLanguages: ResolveAppLanguagesGuard
                },
                children: [
                    {
                        path: 'new',
                        component: ContentPageComponent,
                        canDeactivate: [CanDeactivateGuard],
                        children: [
                            {
                                path: 'assets',
                                loadChildren: './../assets/module#SqxFeatureAssetsModule'
                            }
                        ]
                    },
                    {
                        path: ':contentId',
                        component: ContentPageComponent,
                        canDeactivate: [CanDeactivateGuard],
                        resolve: {
                            content: ResolveContentGuard
                        },
                        children: [
                             {
                                path: 'history',
                                component: HistoryComponent,
                                data: {
                                    channel: 'contents.{contentId}'
                                }
                            },
                            {
                                path: 'assets',
                                loadChildren: './../assets/module#SqxFeatureAssetsModule'
                            }
                        ]
                    }
                ]
            }]
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        ContentFieldComponent,
        ContentItemComponent,
        ContentPageComponent,
        ContentsPageComponent,
        SchemasPageComponent
    ]
})
export class SqxFeatureContentModule { }