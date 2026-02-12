import { bootstrapApplication } from "@angular/platform-browser";
import { ModuleRegistry, AllCommunityModule } from "ag-grid-community";
import { appConfig } from "./app/app.config";
import { App } from "./app/app";
import "bootstrap";

ModuleRegistry.registerModules([AllCommunityModule]);

bootstrapApplication(App, appConfig).catch((err) => console.error(err));
