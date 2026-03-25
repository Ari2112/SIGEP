Table Schedules {
  Id int [pk]
}

Table Positions {
  Id int [pk]
}

Table PermissionTypes {
  Id int [pk]
}

Table DeductionTypes {
  Id int [pk]
}

Table BenefitTypes {
  Id int [pk]
}

Table Employees {
  Id int [pk]
  PositionId int [ref: > Positions.Id]
  ScheduleId int [ref: > Schedules.Id]
  SupervisorId int [ref: > Employees.Id]
}

Table Users {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
}

Table AuditLogs {
  Id bigint [pk]
  UserId int [ref: > Users.Id]
}

Table Notifications {
  Id int [pk]
  UserId int [ref: > Users.Id]
}

Table VacationBalances {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
}

Table VacationRequests {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
  ApprovedByUserId int [ref: > Users.Id]
}

Table VacationRequestHistory {
  Id int [pk]
  VacationRequestId int [ref: > VacationRequests.Id]
  ChangedByUserId int [ref: > Users.Id]
}

Table PermissionRequests {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
  PermissionTypeId int [ref: > PermissionTypes.Id]
  ApprovedByUserId int [ref: > Users.Id]
}

Table AttendanceRecords {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
}

Table OvertimeRecords {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
  AttendanceId int [ref: > AttendanceRecords.Id]
  ReviewedById int [ref: > Users.Id]
  PayrollDetailId int
}

Table Payrolls {
  Id int [pk]
  ProcessedById int [ref: > Users.Id]
  ApprovedById int [ref: > Users.Id]
}

Table PayrollDetails {
  Id int [pk]
  PayrollId int [ref: > Payrolls.Id]
  EmployeeId int [ref: > Employees.Id]
}

Table PayrollDeductions {
  Id int [pk]
  PayrollDetailId int [ref: > PayrollDetails.Id]
  DeductionTypeId int [ref: > DeductionTypes.Id]
}

Table PayrollBenefits {
  Id int [pk]
  PayrollDetailId int [ref: > PayrollDetails.Id]
  BenefitTypeId int [ref: > BenefitTypes.Id]
}

Table Settlements {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
  CalculatedById int [ref: > Users.Id]
  ApprovedById int [ref: > Users.Id]
}

Table SettlementDeductions {
  Id int [pk]
  SettlementId int [ref: > Settlements.Id]
}

Table AnnualBonuses {
  Id int [pk]
  CalculatedById int [ref: > Users.Id]
  ApprovedById int [ref: > Users.Id]
}

Table AnnualBonusDetails {
  Id int [pk]
  AnnualBonusId int [ref: > AnnualBonuses.Id]
  EmployeeId int [ref: > Employees.Id]
}

Table PerformanceEvaluations {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
  EvaluatorId int [ref: > Users.Id]
}

Table DisabilityRequests {
  Id int [pk]
  EmployeeId int [ref: > Employees.Id]
  ReviewedById int [ref: > Users.Id]
}

Table SystemSettings {
  Id int [pk]
}