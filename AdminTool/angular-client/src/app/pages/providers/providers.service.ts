import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";

export interface ProviderItem {
  provider: string;
  planCount: number;
  latestEffectiveDate: string;
}

export interface ProviderDetailItem {
  provider: string;
  planCount: number;
  earliestEffectiveDate: string;
  latestEffectiveDate: string;
  activePlans: number;
  pendingPlans: number;
  expiredPlans: number;
  averageMonthlyPremium: number;
  averageDeductible: number;
  averageOutOfPocketMax: number;
  planTypes: string[];
  members: string[];
  recentNotes: string[];
}

interface ProviderListResponse {
  items: ProviderItem[];
}

interface ProviderItemResponse {
  item: ProviderDetailItem;
}

@Injectable({ providedIn: "root" })
export class ProvidersService {
  private readonly baseUrl = "/api/providers";

  constructor(private readonly http: HttpClient) {}

  list(sort?: Array<{ field: string; direction: "asc" | "desc" }>, query?: string) {
    let params = new HttpParams();

    for (const item of sort ?? []) {
      params = params.append("sort", `${item.field}:${item.direction}`);
    }

    const searchQuery = (query ?? "").trim();
    if (searchQuery) {
      params = params.set("query", searchQuery);
    }

    return this.http.get<ProviderListResponse>(this.baseUrl, { params });
  }

  getByProvider(provider: string) {
    return this.http.get<ProviderItemResponse>(`${this.baseUrl}/${encodeURIComponent(provider)}`);
  }
}
