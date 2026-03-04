import { CommonModule } from "@angular/common";
import { Component, EventEmitter, Input, Output } from "@angular/core";
import { AgGridModule } from "ag-grid-angular";
import {
  AllCommunityModule,
  CellClickedEvent,
  ColDef,
  GridApi,
  GridReadyEvent,
  PaginationChangedEvent,
  RowClickedEvent,
} from "ag-grid-community";

export interface DataGridColumn {
  key: string;
  label: string;
  type?: "text" | "dateTime";
  width?: number;
  minWidth?: number;
  flex?: number;
}

@Component({
  selector: "app-data-grid",
  standalone: true,
  imports: [CommonModule, AgGridModule],
  templateUrl: "./data-grid.component.html",
  styleUrl: "./data-grid.component.scss",
})
export class DataGridComponent {
  private static readonly MAX_PAGE_SIZE = 50;
  private gridApi: GridApi | null = null;
  private isEditingPageInput = false;

  currentPage = 1;
  totalPages = 1;
  pageInput = "1";

  @Input() columns: DataGridColumn[] = [];
  @Input() columnDefs: ColDef[] | null = null;
  @Input() rows: object[] = [];
  @Input() pageSize = 50;
  @Input() pageSizeOptions: number[] | false = false;
  @Input() defaultColDef: ColDef | null = null;
  @Input() rowHeight = 30;
  @Input() headerHeight = 30;
  @Input() alwaysShowHorizontalScroll = true;
  @Input() suppressHorizontalScroll = false;

  @Output() rowClicked = new EventEmitter<RowClickedEvent>();
  @Output() cellClicked = new EventEmitter<CellClickedEvent>();
  @Output() gridReady = new EventEmitter<GridReadyEvent>();

  modules = [AllCommunityModule];
  readonly baseDefaultColDef: ColDef = {
    sortable: true,
    filter: true,
    resizable: true,
    minWidth: 140,
    flex: 1,
  };

  get resolvedDefaultColDef(): ColDef {
    return {
      ...this.baseDefaultColDef,
      ...(this.defaultColDef ?? {}),
    };
  }

  get resolvedColumnDefs(): ColDef[] {
    if (this.columnDefs?.length) {
      return this.columnDefs;
    }

    return this.columns.map((column) => ({
      field: column.key,
      headerName: column.label,
      minWidth: column.minWidth,
      width: column.width,
      flex: column.flex,
      valueFormatter: column.type === "dateTime" ? (params) => this.formatDateTime(params.value) : undefined,
      comparator:
        column.type === "dateTime"
          ? (a: string, b: string) => {
              const left = this.toEpoch(a);
              const right = this.toEpoch(b);
              return left - right;
            }
          : undefined,
    }));
  }

  get shouldPaginate() {
    return this.rows.length > this.effectivePageSize;
  }

  get effectivePageSize() {
    return Math.min(this.pageSize, DataGridComponent.MAX_PAGE_SIZE);
  }

  get effectivePageSizeOptions(): number[] | false {
    if (!this.pageSizeOptions || this.pageSizeOptions.length === 0) {
      return false;
    }

    const normalized = this.pageSizeOptions
      .map((value) => Number(value))
      .filter((value) => Number.isFinite(value) && value > 0 && value <= DataGridComponent.MAX_PAGE_SIZE);

    if (!normalized.length) {
      return false;
    }

    const uniqueSorted = Array.from(new Set(normalized)).sort((a, b) => a - b);
    if (!uniqueSorted.includes(this.effectivePageSize)) {
      uniqueSorted.push(this.effectivePageSize);
      uniqueSorted.sort((a, b) => a - b);
    }

    return uniqueSorted;
  }

  onGridReady(event: GridReadyEvent) {
    this.gridApi = event.api;
    this.syncPaginationState();
    this.gridReady.emit(event);
  }

  onPaginationChanged(_: PaginationChangedEvent) {
    this.syncPaginationState();
  }

  get canGoPrevious() {
    return this.currentPage > 1;
  }

  get canGoNext() {
    return this.currentPage < this.totalPages;
  }

  get rowCount() {
    return this.rows.length;
  }

  get startRow() {
    if (!this.shouldPaginate || this.rowCount === 0) {
      return this.rowCount === 0 ? 0 : 1;
    }

    return (this.currentPage - 1) * this.effectivePageSize + 1;
  }

  get endRow() {
    if (!this.shouldPaginate) {
      return this.rowCount;
    }

    return Math.min(this.currentPage * this.effectivePageSize, this.rowCount);
  }

  goToFirstPage() {
    this.goToPage(1);
  }

  goToPreviousPage() {
    this.goToPage(this.currentPage - 1);
  }

  goToNextPage() {
    this.goToPage(this.currentPage + 1);
  }

  goToLastPage() {
    this.goToPage(this.totalPages);
  }

  commitPageInput() {
    this.isEditingPageInput = false;

    const parsed = Number(this.pageInput);
    if (!Number.isFinite(parsed)) {
      this.pageInput = String(this.currentPage);
      return;
    }

    this.goToPage(Math.trunc(parsed));
  }

  onPageInputFocus() {
    this.isEditingPageInput = true;
  }

  onPageInputBlur() {
    this.commitPageInput();
  }

  handleGridWheel(event: WheelEvent) {
    const shell = event.currentTarget as HTMLElement | null;
    if (!shell) {
      return;
    }

    const horizontalViewport = shell.querySelector(".ag-body-horizontal-scroll-viewport") as HTMLElement | null;
    if (!horizontalViewport) {
      return;
    }

    if (horizontalViewport.scrollWidth <= horizontalViewport.clientWidth) {
      return;
    }

    const delta = Math.abs(event.deltaX) > 0 ? event.deltaX : event.deltaY;
    horizontalViewport.scrollLeft += delta;
    event.preventDefault();
  }

  private goToPage(pageNumber: number) {
    if (!this.gridApi || !this.shouldPaginate) {
      return;
    }

    const clampedPage = this.clampPage(pageNumber);
    this.gridApi.paginationGoToPage(clampedPage - 1);
    this.syncPaginationState();
  }

  private clampPage(pageNumber: number) {
    if (this.totalPages <= 1) {
      return 1;
    }

    if (!Number.isFinite(pageNumber)) {
      return this.currentPage;
    }

    return Math.min(Math.max(pageNumber, 1), this.totalPages);
  }

  private syncPaginationState() {
    if (!this.gridApi || !this.shouldPaginate) {
      this.currentPage = 1;
      this.totalPages = 1;
      this.pageInput = "1";
      return;
    }

    this.currentPage = this.gridApi.paginationGetCurrentPage() + 1;
    const pageCount = this.gridApi.paginationGetTotalPages();
    this.totalPages = pageCount > 0 ? pageCount : 1;

    if (!this.isEditingPageInput) {
      this.pageInput = String(this.currentPage);
    }
  }

  private formatDateTime(value: string | null | undefined) {
    if (!value) {
      return "-";
    }

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
    }).format(parsed);
  }

  private toEpoch(value: string | null | undefined) {
    if (!value) {
      return 0;
    }

    const parsed = new Date(value).getTime();
    return Number.isNaN(parsed) ? 0 : parsed;
  }
}
