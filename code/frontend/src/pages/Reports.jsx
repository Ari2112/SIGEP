import { useState, useEffect } from 'react';
import { reportAPI, payrollAPI, employeeAPI } from '../api/api';
import Layout from '../components/Layout';
import './Reports.css';

const API_URL = 'http://localhost:5017/api/v1';
const MONTHS = ['Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'];

function Reports() {
  const [activeTab, setActiveTab] = useState('attendance');
  const [attendanceReport, setAttendanceReport] = useState([]);
  const [overtimeReport, setOvertimeReport] = useState([]);
  const [payrollReport, setPayrollReport] = useState(null);
  const [payrolls, setPayrolls] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [selectedPayroll, setSelectedPayroll] = useState('');
  const [selectedEmployee, setSelectedEmployee] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [filters, setFilters] = useState({
    dateFrom: new Date(new Date().setDate(1)).toISOString().split('T')[0],
    dateTo: new Date().toISOString().split('T')[0]
  });

  useEffect(() => {
    loadPayrolls();
    loadEmployees();
  }, []);

  const loadPayrolls = async () => {
    try {
      const res = await payrollAPI.getAll();
      setPayrolls(res.data);
    } catch (_) {}
  };

  const loadEmployees = async () => {
    try {
      const res = await employeeAPI.getAll();
      setEmployees(res.data);
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

  // Descarga PDF con token JWT
  const downloadWithToken = async (url, filename) => {
    setError('');
    try {
      const token = localStorage.getItem('token');
      const res = await fetch(url, { headers: { Authorization: `Bearer ${token}` } });

      if (!res.ok) {
        // El backend devuelve JSON con el detalle del error
        let msg = `Error ${res.status} al generar el PDF`;
        try {
          const data = await res.json();
          if (data?.message) msg = data.message;
        } catch (_) { /* respuesta no-JSON */ }
        setError(msg);
        return;
      }

      const blob = await res.blob();
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = filename;
      a.click();
      URL.revokeObjectURL(a.href);
    } catch (err) {
      setError('No se pudo descargar el PDF: ' + (err?.message || 'error de conexión'));
    }
  };

  const downloadPayrollPdf = () => {
    if (!selectedPayroll) { setError('Seleccione una planilla'); return; }
    const planilla = payrolls.find(p => p.id === parseInt(selectedPayroll));
    const nombre = planilla
      ? `Planilla_${MONTHS[planilla.periodMonth - 1]}_${planilla.periodYear}.pdf`
      : `Planilla_${selectedPayroll}.pdf`;
    downloadWithToken(`${API_URL}/payroll/${selectedPayroll}/pdf`, nombre);
  };

  const downloadPayslipPdf = () => {
    if (!selectedPayroll) { setError('Seleccione una planilla'); return; }
    if (!selectedEmployee) { setError('Seleccione un empleado para la colilla individual'); return; }
    const emp = (payrollReport?.employees || []).find(e => e.employeeId === parseInt(selectedEmployee));
    const nombre = emp
      ? `Colilla_${emp.employeeName?.replace(/ /g, '_')}.pdf`
      : `Colilla_${selectedEmployee}.pdf`;
    downloadWithToken(
      `${API_URL}/payroll/${selectedPayroll}/pdf/employee/${selectedEmployee}`,
      nombre
    );
  };

  const formatCurrency = (v) =>
    new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v || 0);

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

        {/* Filtros asistencia y horas extra */}
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
                  <th>Tardías</th>
                  <th>Min. Tarde</th>
                  <th>% Asistencia</th>
                </tr>
              </thead>
              <tbody>
                {attendanceReport.length === 0 ? (
                  <tr><td colSpan="12" className="no-data">
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
                        {r.lateDays > 0
                          ? <span className="badge badge-warning">{r.lateDays}</span>
                          : <span className="badge badge-success">0</span>}
                      </td>
                      <td>
                        {r.totalLateMinutes > 0
                          ? <span style={{color:'#e74c3c'}}>{r.totalLateMinutes} min</span>
                          : '-'}
                      </td>
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
                  <th className="num">Total Registros</th>
                  <th className="num">Total Horas</th>
                  <th className="num">Total Monto</th>
                  <th className="num">Aprobados</th>
                  <th className="num">Horas Aprobadas</th>
                  <th className="num">Monto Aprobado</th>
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
                      <td className="num">{r.totalRecords}</td>
                      <td className="num">{r.totalHours}h</td>
                      <td className="num">{formatCurrency(r.totalAmount)}</td>
                      <td className="num"><span className="badge badge-success">{r.approvedRecords}</span></td>
                      <td className="num">{r.approvedHours}h</td>
                      <td className="num"><strong>{formatCurrency(r.approvedAmount)}</strong></td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}

        {/* Reporte Planilla con PDF */}
        {activeTab === 'payroll' && (
          <>
            {/* Filtros */}
            <div className="filters-bar" style={{ flexWrap: 'wrap', gap: '10px' }}>
              <select value={selectedPayroll} onChange={e => { setSelectedPayroll(e.target.value); setPayrollReport(null); }}>
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

              <button
                className="btn btn-secondary"
                onClick={downloadPayrollPdf}
                disabled={!selectedPayroll}
                title="Descarga PDF con todos los empleados"
              >
                PDF General
              </button>
            </div>

            {/* Selector de empleado para colilla individual (solo empleados de la planilla) */}
            {selectedPayroll && payrollReport && (
              <div className="filters-bar" style={{ marginTop: '8px', flexWrap: 'wrap', gap: '10px' }}>
                <label style={{ fontWeight: '500' }}>Colilla individual:</label>
                <select value={selectedEmployee} onChange={e => setSelectedEmployee(e.target.value)}>
                  <option value="">Seleccionar empleado...</option>
                  {(payrollReport.employees || []).map(e => (
                    <option key={e.employeeId} value={e.employeeId}>
                      {e.employeeName}
                    </option>
                  ))}
                </select>
                <button
                  className="btn btn-secondary"
                  onClick={downloadPayslipPdf}
                  disabled={!selectedEmployee}
                  title="Descarga colilla individual del empleado seleccionado"
                >
                  Descargar Colilla
                </button>
              </div>
            )}

            {selectedPayroll && !payrollReport && (
              <p className="hint-text" style={{ marginTop: '8px', color: '#888' }}>
                Pulse "Ver Reporte" para cargar la planilla y poder descargar colillas individuales.
              </p>
            )}

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
                        <th className="num">Salario Base</th>
                        <th className="num">Horas Extra</th>
                        <th className="num">Bruto</th>
                        <th className="num">CCSS Obrero</th>
                        <th className="num">Imp. Renta</th>
                        <th className="num">Total Ded.</th>
                        <th className="num">Neto</th>
                        <th>Colilla</th>
                      </tr>
                    </thead>
                    <tbody>
                      {payrollReport.employees.map((e, i) => {
                        const ccss = (e.deductions || [])
                          .filter(d => d.deductionTypeName?.includes('CCSS') || d.deductionTypeName?.includes('Banco'))
                          .reduce((sum, d) => sum + d.amount, 0);
                        const renta = (e.deductions || [])
                          .filter(d => d.deductionTypeName?.includes('Renta'))
                          .reduce((sum, d) => sum + d.amount, 0);
                        return (
                          <tr key={i}>
                            <td><strong>{e.employeeName}</strong></td>
                            <td>{e.positionName || '-'}</td>
                            <td className="num">{formatCurrency(e.baseSalary)}</td>
                            <td className="num">{e.overtimeAmount > 0 ? formatCurrency(e.overtimeAmount) : '-'}</td>
                            <td className="num">{formatCurrency(e.grossSalary)}</td>
                            <td className="num" style={{color:'#c0392b'}}>{ccss > 0 ? formatCurrency(ccss) : '-'}</td>
                            <td className="num" style={{color:'#c0392b'}}>{renta > 0 ? formatCurrency(renta) : '-'}</td>
                            <td className="num" style={{color:'#c0392b'}}>{formatCurrency(e.totalDeductions)}</td>
                            <td className="num"><strong style={{color:'#1a6b1a'}}>{formatCurrency(e.netSalary)}</strong></td>
                            <td>
                              <button
                                className="btn btn-sm btn-secondary"
                                onClick={() => {
                                  setSelectedEmployee(e.employeeId);
                                  downloadWithToken(
                                    `${API_URL}/payroll/${selectedPayroll}/pdf/employee/${e.employeeId}`,
                                    `Colilla_${e.employeeName?.replace(/ /g, '_')}.pdf`
                                  );
                                }}
                              >
                                PDF
                              </button>
                            </td>
                          </tr>
                        );
                      })}
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