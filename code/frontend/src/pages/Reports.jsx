import { useState, useEffect } from 'react';
import { reportAPI, payrollAPI } from '../api/api';
import Layout from '../components/Layout';
import './Reports.css';

function Reports() {
  const [activeTab, setActiveTab] = useState('attendance');
  const [attendanceReport, setAttendanceReport] = useState([]);
  const [overtimeReport, setOvertimeReport] = useState([]);
  const [payrollReport, setPayrollReport] = useState(null);
  const [payrolls, setPayrolls] = useState([]);
  const [selectedPayroll, setSelectedPayroll] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [filters, setFilters] = useState({
    dateFrom: new Date(new Date().setDate(1)).toISOString().split('T')[0],
    dateTo: new Date().toISOString().split('T')[0]
  });

  useEffect(() => { loadPayrolls(); }, []);

  const loadPayrolls = async () => {
    try {
      const res = await payrollAPI.getAll();
      setPayrolls(res.data);
    } catch (_) {}
  };

  const loadAttendanceReport = async () => {
    try {
      setLoading(true);
      setError('');
      const res = await reportAPI.getAttendanceReport(filters);
      setAttendanceReport(res.data);
    } catch (err) {
      setError('Error al generar reporte: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const loadOvertimeReport = async () => {
    try {
      setLoading(true);
      setError('');
      const res = await reportAPI.getOvertimeReport(filters);
      setOvertimeReport(res.data);
    } catch (err) {
      setError('Error al generar reporte: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const loadPayrollReport = async () => {
    if (!selectedPayroll) { setError('Seleccione una planilla'); return; }
    try {
      setLoading(true);
      setError('');
      const res = await reportAPI.getPayrollReport(selectedPayroll);
      setPayrollReport(res.data);
    } catch (err) {
      setError('Error al generar reporte: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (v) => new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v || 0);

  const MONTHS = ['Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'];

  return (
    <Layout>
      <div className="reports-container">
        <div className="page-header">
          <div>
            <h1>Reportes</h1>
            <p className="page-subtitle">Informes de asistencia, horas extra y planilla</p>
          </div>
        </div>

        {error && <div className="alert alert-error">{error}</div>}

        <div className="tabs">
          <button className={`tab ${activeTab === 'attendance' ? 'active' : ''}`} onClick={() => setActiveTab('attendance')}>
            Asistencia
          </button>
          <button className={`tab ${activeTab === 'overtime' ? 'active' : ''}`} onClick={() => setActiveTab('overtime')}>
            Horas Extra
          </button>
          <button className={`tab ${activeTab === 'payroll' ? 'active' : ''}`} onClick={() => setActiveTab('payroll')}>
            Planilla Consolidada
          </button>
        </div>

        {/* Filtro de fechas (asistencia y horas extra) */}
        {(activeTab === 'attendance' || activeTab === 'overtime') && (
          <div className="filters-bar">
            <label>Desde:</label>
            <input type="date" value={filters.dateFrom}
              onChange={e => setFilters({ ...filters, dateFrom: e.target.value })} />
            <label>Hasta:</label>
            <input type="date" value={filters.dateTo}
              onChange={e => setFilters({ ...filters, dateTo: e.target.value })} />
            <button className="btn btn-primary"
              onClick={activeTab === 'attendance' ? loadAttendanceReport : loadOvertimeReport}
              disabled={loading}>
              {loading ? 'Generando...' : 'Generar Reporte'}
            </button>
          </div>
        )}

        {/* Reporte Asistencia */}
        {activeTab === 'attendance' && (
          <div className="table-card">
            <table className="table">
              <thead>
                <tr>
                  <th>Empleado</th>
                  <th>Puesto</th>
                  <th>Días Periodo</th>
                  <th>Presentes</th>
                  <th>Ausentes</th>
                  <th>Permisos</th>
                  <th>Vacaciones</th>
                  <th>Incapacidades</th>
                  <th>Horas Trab.</th>
                  <th>% Asistencia</th>
                </tr>
              </thead>
              <tbody>
                {attendanceReport.length === 0 ? (
                  <tr><td colSpan="10" className="no-data">
                    {loading ? 'Cargando...' : 'Seleccione un período y genere el reporte'}
                  </td></tr>
                ) : (
                  attendanceReport.map(r => (
                    <tr key={r.employeeId}>
                      <td><strong>{r.employeeName}</strong></td>
                      <td>{r.positionName || '-'}</td>
                      <td>{r.totalDays}</td>
                      <td><span className="badge badge-success">{r.presentDays}</span></td>
                      <td><span className="badge badge-danger">{r.absentDays}</span></td>
                      <td>{r.permissionDays}</td>
                      <td>{r.vacationDays}</td>
                      <td>{r.disabilityDays}</td>
                      <td>{r.totalWorkedHours}h</td>
                      <td>
                        <div className="progress-bar">
                          <div className="progress-fill" style={{ width: `${r.attendanceRate}%` }} />
                        </div>
                        <span className="progress-text">{r.attendanceRate}%</span>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Reporte Horas Extra */}
        {activeTab === 'overtime' && (
          <div className="table-card">
            <table className="table">
              <thead>
                <tr>
                  <th>Empleado</th>
                  <th>Puesto</th>
                  <th>Total Registros</th>
                  <th>Total Horas</th>
                  <th>Total Monto</th>
                  <th>Aprobados</th>
                  <th>Horas Aprobadas</th>
                  <th>Monto Aprobado</th>
                </tr>
              </thead>
              <tbody>
                {overtimeReport.length === 0 ? (
                  <tr><td colSpan="8" className="no-data">
                    {loading ? 'Cargando...' : 'Seleccione un período y genere el reporte'}
                  </td></tr>
                ) : (
                  overtimeReport.map(r => (
                    <tr key={r.employeeId}>
                      <td><strong>{r.employeeName}</strong></td>
                      <td>{r.positionName || '-'}</td>
                      <td>{r.totalRecords}</td>
                      <td>{r.totalHours}h</td>
                      <td>{formatCurrency(r.totalAmount)}</td>
                      <td><span className="badge badge-success">{r.approvedRecords}</span></td>
                      <td>{r.approvedHours}h</td>
                      <td><strong>{formatCurrency(r.approvedAmount)}</strong></td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Reporte Planilla */}
        {activeTab === 'payroll' && (
          <>
            <div className="filters-bar">
              <select value={selectedPayroll}
                onChange={e => setSelectedPayroll(e.target.value)}>
                <option value="">Seleccione una planilla...</option>
                {payrolls.map(p => (
                  <option key={p.id} value={p.id}>
                    {MONTHS[p.periodMonth - 1]} {p.periodYear} — {p.periodType} ({p.status})
                  </option>
                ))}
              </select>
              <button className="btn btn-primary" onClick={loadPayrollReport} disabled={loading || !selectedPayroll}>
                {loading ? 'Generando...' : 'Ver Reporte'}
              </button>
            </div>

            {payrollReport && (
              <>
                <div className="report-summary-grid">
                  <div className="report-card">
                    <span className="report-label">Empleados</span>
                    <span className="report-value">{payrollReport.totalEmployees}</span>
                  </div>
                  <div className="report-card">
                    <span className="report-label">Salario Bruto</span>
                    <span className="report-value">{formatCurrency(payrollReport.totalGrossSalary)}</span>
                  </div>
                  <div className="report-card danger">
                    <span className="report-label">Deducciones</span>
                    <span className="report-value">{formatCurrency(payrollReport.totalDeductions)}</span>
                  </div>
                  <div className="report-card success">
                    <span className="report-label">Total Neto</span>
                    <span className="report-value">{formatCurrency(payrollReport.totalNetSalary)}</span>
                  </div>
                </div>

                <div className="table-card">
                  <table className="table">
                    <thead>
                      <tr>
                        <th>Empleado</th>
                        <th>Puesto</th>
                        <th>Salario Base</th>
                        <th>Horas Extra</th>
                        <th>Bruto</th>
                        <th>Deducciones</th>
                        <th>Neto</th>
                      </tr>
                    </thead>
                    <tbody>
                      {payrollReport.employees.map((e, i) => (
                        <tr key={i}>
                          <td>{e.employeeName}</td>
                          <td>{e.positionName || '-'}</td>
                          <td>{formatCurrency(e.baseSalary)}</td>
                          <td>{formatCurrency(e.overtimeAmount)}</td>
                          <td>{formatCurrency(e.grossSalary)}</td>
                          <td>{formatCurrency(e.totalDeductions)}</td>
                          <td><strong>{formatCurrency(e.netSalary)}</strong></td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </>
            )}
          </>
        )}
      </div>
    </Layout>
  );
}

export default Reports;
