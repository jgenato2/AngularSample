import { Injectable } from "@angular/core";
import { CreateClaimCommand } from "./commands/create-claim.command";
import { DeleteClaimCommand } from "./commands/delete-claim.command";
import { UpdateClaimCommand } from "./commands/update-claim.command";
import { GetClaimAuditLogsQuery } from "./queries/get-claim-audit-logs.query";
import { GetClaimByIdQuery } from "./queries/get-claim-by-id.query";
import { GetClaimsListAccessAuditLogsQuery } from "./queries/get-claims-list-access-audit-logs.query";
import { GetClaimStatusWorkflowQuery } from "./queries/get-claim-status-workflow.query";
import { ListClaimsQuery } from "./queries/list-claims.query";
import { CreateClaimPayload, UpdateClaimPayload } from "../domain/claim.models";

@Injectable({ providedIn: "root" })
export class ClaimsFacade {
  constructor(
    private readonly listClaimsQuery: ListClaimsQuery,
    private readonly getClaimByIdQuery: GetClaimByIdQuery,
    private readonly getClaimStatusWorkflowQuery: GetClaimStatusWorkflowQuery,
    private readonly getClaimAuditLogsQuery: GetClaimAuditLogsQuery,
    private readonly getClaimsListAccessAuditLogsQuery: GetClaimsListAccessAuditLogsQuery,
    private readonly createClaimCommand: CreateClaimCommand,
    private readonly updateClaimCommand: UpdateClaimCommand,
    private readonly deleteClaimCommand: DeleteClaimCommand
  ) {}

  list(sort?: Array<{ field: string; direction: "asc" | "desc" }>) {
    return this.listClaimsQuery.execute(sort);
  }

  getById(claimId: string) {
    return this.getClaimByIdQuery.execute(claimId);
  }

  getStatusWorkflow() {
    return this.getClaimStatusWorkflowQuery.execute();
  }

  getAuditLogs(claimId: string) {
    return this.getClaimAuditLogsQuery.execute(claimId);
  }

  getListAccessAuditLogs() {
    return this.getClaimsListAccessAuditLogsQuery.execute();
  }

  create(payload: CreateClaimPayload) {
    return this.createClaimCommand.execute(payload);
  }

  update(claimId: string, payload: UpdateClaimPayload) {
    return this.updateClaimCommand.execute(claimId, payload);
  }

  remove(claimId: string) {
    return this.deleteClaimCommand.execute(claimId);
  }
}
