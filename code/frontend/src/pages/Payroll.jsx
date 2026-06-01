import { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { payrollAPI } from '../api/api';
import Layout from '../components/Layout';
import './Payroll.css';

const MONTHS = ['Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto','Septiembre','Octubre','Noviembre','Diciembre'];
const PERIOD_TYPES = { PrimeraQuincena: 'Primera Quincena', SegundaQuincena: 'Segunda Quincena', Mensual: 'Mensual' };
const STATUS_COLORS = { Borrador: 'badge-secondary', Procesando: 'badge-warning', Completada: 'badge-success', Anulada: 'badge-danger' };

const API_URL = 'http://localhost:5017/api/v1';

function Payroll() {
  const { user } = useAuth();
  const [payrolls, setPayrolls] = useState([]);
  const [selected, setSelected] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showGenerate, setShowGenerate] = useState(false);
  const [showDetail, setShowDetail] = useState(false);
  const [generating, setGenerating] = useState(false);
  const [form, setForm] = useState({
    periodYear: new Date().getFullYear(),
    periodMonth: new Date().getMonth() + 1,
    periodType: 1,
    notes: ''
  });

  useEffect(() => { loadPayrolls(); }, []);

  const loadPayrolls = async () => {
    try {
      setLoading(true);
      const res = await payrollAPI.getAll();
      setPayrolls(res.data);
    } catch (err) {
      setError('Error al cargar planillas: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const handleGenerate = async () => {
    try {
      setGenerating(true);
      setError('');
      await payrollAPI.generate(form);
      setSuccess('Planilla generada exitosamente');
      setShowGenerate(false);
      loadPayrolls();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al generar planilla');
    } finally {
      setGenerating(false);
    }
  };

  const handleApprove = async (id) => {
    try {
      await payrollAPI.approve(id, '');
      setSuccess('Planilla aprobada');
      loadPayrolls();
      if (selected?.id === id) {
        const res = await payrollAPI.getById(id);
        setSelected(res.data);
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Error al aprobar');
    }
  };

  const handleViewDetail = async (id) => {
    try {
      const res = await payrollAPI.getById(id);
      setSelected(res.data);
      setShowDetail(true);
    } catch (err) {
      setError('Error al cargar detalle');
    }
  };

  const downloadPdf = (payrollId) => {
    const token = localStorage.getItem('token');
    fetch(`${API_URL}/payroll/${payrollId}/pdf`, {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(res => res.blob())
      .then(blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Planilla_${payrollId}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
      })
      .catch(() => setError('Error al generar PDF'));
  };

  const downloadPayslip = (payrollId, employeeId, employeeName) => {
    const token = localStorage.getItem('token');
    fetch(`${API_URL}/payroll/${payrollId}/pdf/employee/${employeeId}`, {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then(res => res.blob())
      .then(blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Colilla_${employeeName?.replace(/ /g, '_')}_${payrollId}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
      })
      .catch(() => setError('Error al generar colilla'));
  };

  const formatCurrency = (v) => new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v);
  const formatDate = (d) => d ? new Date(d).toLocaleDateString('es-CR') : '-';

  if (loading) return <Layout><div className="loading">Cargando planillas...</div></Layout>;

  return (
    <Layout>
      <div className="payroll-container">
        <div className="page-header">
          <div>
            <h1>Planilla</h1>
            <p className="page-subtitle">Gestión de planillas quincenales y mensuales</p>
          </div>
          <button className="btn btn-primary" onClick={() => { setShowGenerate(true); setError(''); }}>
            + Generar Planilla
          </button>
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        <div className="table-card">
          <table className="table">
            <thead>
              <tr>
                <th>Período</th>
                <th>Tipo</th>
                <th>Empleados</th>
                <th>Salario Bruto</th>
                <th>Deducciones</th>
                <th>Salario Neto</th>
                <th>Estado</th>
                <th>Generado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {payrolls.length === 0 ? (
                <tr><td colSpan="9" className="no-data">No hay planillas generadas</td></tr>
              ) : (
                payrolls.map(p => (
                  <tr key={p.id}>
                    <td><strong>{MONTHS[p.periodMonth - 1]} {p.periodYear}</strong></td>
                    <td><span className="badge badge-info">{PERIOD_TYPES[p.periodType] || p.periodType}</span></td>
                    <td>{p.totalEmployees}</td>
                    <td>{formatCurrency(p.totalGrossSalary)}</td>
                    <td>{formatCurrency(p.totalDeductions)}</td>
                    <td><strong>{formatCurrency(p.totalNetSalary)}</strong></td>
                    <td><span className={`badge ${STATUS_COLORS[p.status] || 'badge-secondary'}`}>{p.status}</span></td>
                    <td>{formatDate(p.createdAt)}</td>
                    <td style={{ display: 'flex', gap: '4px', flexWrap: 'wrap' }}>
                      <button className="btn btn-sm btn-ghost" onClick={() => handleViewDetail(p.id)}>Ver</button>
                      <button className="btn btn-sm btn-primary" onClick={() => downloadPdf(p.id)}>PDF</button>
                      {p.status === 'Completada' && !p.approvedAt && (
                        <button className="btn btn-sm btn-success" onClick={() => handleApprove(p.id)}>Aprobar</button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Modal Generar */}
        {showGenerate && (
          <div className="modal-overlay" onClick={() => setShowGenerate(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Generar Nueva Planilla</h2>
                <button className="close-btn" onClick={() => setShowGenerate(false)}>&times;</button>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Año</label>
                  <input type="number" value={form.periodYear}
                    onChange={e => setForm({ ...form, periodYear: parseInt(e.target.value) })}
                    min="2020" max="2030" />
                </div>
                <div className="form-group">
                  <label>Mes</label>
                  <select value={form.periodMonth}
                    onChange={e => setForm({ ...form, periodMonth: parseInt(e.target.value) })}>
                    {MONTHS.map((m, i) => <option key={i} value={i + 1}>{m}</option>)}
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label>Tipo de Período</label>
                <select value={form.periodType}
                  onChange={e => setForm({ ...form, periodType: parseInt(e.target.value) })}>
                  <option value={1}>Primera Quincena (1-15)</option>
                  <option value={2}>Segunda Quincena (16-fin)</option>
                  <option value={3}>Mensual (mes completo)</option>
                </select>
              </div>
              <div className="form-group">
                <label>Notas (opcional)</label>
                <textarea rows="2" value={form.notes}
                  onChange={e => setForm({ ...form, notes: e.target.value })}
                  placeholder="Notas adicionales..." />
              </div>
              <div className="alert alert-info">
                Se calculará automáticamente el salario para todos los empleados activos incluyendo horas extra aprobadas.
              </div>
              {error && <div className="alert alert-error">{error}</div>}
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowGenerate(false)}>Cancelar</button>
                <button className="btn btn-primary" onClick={handleGenerate} disabled={generating}>
                  {generating ? 'Generando...' : 'Generar Planilla'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Modal Detalle */}
        {showDetail && selected && (
          <div className="modal-overlay" onClick={() => setShowDetail(false)}>
            <div className="modal modal-lg" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Planilla — {MONTHS[selected.periodMonth - 1]} {selected.periodYear} ({PERIOD_TYPES[selected.periodType] || selected.periodType})</h2>
                <button className="close-btn" onClick={() => setShowDetail(false)}>&times;</button>
              </div>

              <div className="payroll-summary-grid">
                <div className="summary-card">
                  <span className="summary-label">Empleados</span>
                  <span className="summary-value">{selected.totalEmployees}</span>
                </div>
                <div className="summary-card">
                  <span className="summary-label">Salario Bruto</span>
                  <span className="summary-value">{formatCurrency(selected.totalGrossSalary)}</span>
                </div>
                <div className="summary-card danger">
                  <span className="summary-label">Deducciones</span>
                  <span className="summary-value">{formatCurrency(selected.totalDeductions)}</span>
                </div>
                <div className="summary-card success">
                  <span className="summary-label">Salario Neto</span>
                  <span className="summary-value">{formatCurrency(selected.totalNetSalary)}</span>
                </div>
              </div>

              <div className="table-card" style={{ marginTop: '16px' }}>
                <table className="table">
                  <thead>
                    <tr>
                      <th>Empleado</th>
                      <th>Puesto</th>
                      <th>Salario Base</th>
                      <th>H.Extra</th>
                      <th>Bruto</th>
                      <th>Deducciones</th>
                      <th>Neto</th>
                      <th>Colilla</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(selected.details || []).map(d => (
                      <tr key={d.id}>
                        <td>{d.employeeName}</td>
                        <td>{d.positionName || '-'}</td>
                        <td>{formatCurrency(d.baseSalary)}</td>
                        <td>{d.overtimeHours > 0 ? `${d.overtimeHours}h (${formatCurrency(d.overtimeAmount)})` : '-'}</td>
                        <td>{formatCurrency(d.grossSalary)}</td>
                        <td>{formatCurrency(d.totalDeductions)}</td>
                        <td><strong>{formatCurrency(d.netSalary)}</strong></td>
                        <td>
                          <button
                            className="btn btn-sm btn-secondary"
                            onClick={() => downloadPayslip(selected.id, d.employeeId, d.employeeName)}
                          >
                            Colilla
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowDetail(false)}>Cerrar</button>
                <button className="btn btn-primary" onClick={() => downloadPdf(selected.id)}>
                  Descargar PDF General
                </button>
                {selected.status === 'Completada' && !selected.approvedAt && (
                  <button className="btn btn-success" onClick={() => { handleApprove(selected.id); setShowDetail(false); }}>
                    Aprobar Planilla
                  </button>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default Payroll;