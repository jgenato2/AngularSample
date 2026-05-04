/**
 * Shared standalone components barrel.
 * Import from here instead of from individual component paths.
 *
 * Example:
 *   import { DataGridComponent, SearchQueryComponent } from '../../shared';
 */
export { DataGridComponent } from "./data-grid/data-grid.component";
export type { GridSortState, DataGridColumn } from "./data-grid/data-grid.component";
export { SearchQueryComponent } from "./search-query/search-query.component";
export { StateLayoutComponent } from "./state-layout/state-layout.component";
export { DetailActionsBarComponent } from "./detail-actions/detail-actions-bar.component";
export { SummaryStatsComponent } from "./summary-stats/summary-stats.component";
export type { SummaryStatItem } from "./summary-stats/summary-stats.component";
