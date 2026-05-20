import { useState, useEffect } from 'react';
import { settlementAPI, employeeAPI } from '../api/api';
import Layout from '../components/Layout';
import './Settlements.css';

const TERMINATION_TYPES = {
  1: 'Renuncia',
  2: 'Despido con responsabilidad',
  3: 'Despido sin responsabilidad',
  4: 'Mutuo acuerdo',
  5: 'Jubilación'
};

const STATUS_COLORS = {
  Borrador: 'badge-secondary',
  Calculada: 'badge-info',
  Aprobada: 'badge-success',
  Pagada: 'badge-warning',
  Anulada: 'badge-danger'
};

function Settlements() {
  const [settlements, setSettlements] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [selected, setSelected] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showCalculate, setShowCalculate] = useState(false);
  const [showDetail, setShowDetail] = useState(false);
  const [calculating, setCalculating] = useState(false);
  const [form, setForm] = useState({
    employeeId: '',
    terminationType: 1,
    terminationDate: new Date().toISOString().split('T')[0],
    notes: '',
    additionalDeductions: []
  });
  const [newDeduction, setNewDeduction] = useState({ description: '', amount: '' });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [settRes, empRes] = await Promise.all([
        settlementAPI.getAll(),
        employeeAPI.getAll()
      ]);
      setSettlements(settRes.data);
      setEmployees(empRes.data.filter(e => e.status === 'Activo' || e.status === 1));
    } catch (err) {
      setError('Error al cargar datos: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const addDeduction = () => {
    if (!newDeduction.description || !newDeduction.amount) return;
    setForm({
      ...form,
      additionalDeductions: [...form.additionalDeductions, {
        description: newDeduction.description,
        amount: parseFloat(newDeduction.amount)
      }]
    });
    setNewDeduction({ description: '', amount: '' });
  };

  const removeDeduction = (idx) => {
    setForm({ ...form, additionalDeductions: form.additionalDeductions.filter((_, i) => i !== idx) });
  };

  const handleCalculate = async () => {
    if (!form.employeeId) { setError('Seleccione un empleado'); return; }
    try {
      setCalculating(true);
      setError('');
      await settlementAPI.calculate({
        employeeId: parseInt(form.employeeId),
        terminationType: parseInt(form.terminationType),
        terminationDate: form.terminationDate,
        notes: form.notes,
        additionalDeductions: form.additionalDeductions
      });
      setSuccess('Liquidación calculada exitosamente');
      setShowCalculate(false);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al calcular liquidación');
    } finally {
      setCalculating(false);
    }
  };

  const handleViewDetail = async (id) => {
    try {
      const res = await settlementAPI.getById(id);
      setSelected(res.data);
      setShowDetail(true);
    } catch (err) {
      setError('Error al cargar detalle');
    }
  };

  const handleApprove = async (id) => {
    try {
      await settlementAPI.approve(id, '');
      setSuccess('Liquidación aprobada');
      loadData();
      if (selected?.id === id) {
        const res = await settlementAPI.getById(id);
        setSelected(res.data);
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Error al aprobar');
    }
  };

  const handleMarkAsPaid = async (id) => {
    try {
      await settlementAPI.markAsPaid(id);
      setSuccess('Liquidación marcada como pagada. El empleado ha sido liquidado.');
      setShowDetail(false);
      loadData();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al marcar como pagada');
    }
  };

  const formatCurrency = (v) => new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v || 0);
  const formatDate = (d) => d ? new Date(d).toLocaleDateString('es-CR') : '-';

  if (loading) return <Layout><div className="loading">Cargando...</div></Layout>;

  return (
    <Layout>
      <div className="settlements-container">
        <div className="page-header">
          <div>
            <h1>Liquidaciones</h1>
            <p className="page-subtitle">Cálculo de prestaciones y liquidaciones de empleados</p>
          </div>
          <button className="btn btn-primary" onClick={() => { setShowCalculate(true); setError(''); }}>
            + Calcular Liquidación
          </button>
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        <div className="table-card">
          <table className="table">
            <thead>
              <tr>
                <th>Empleado</th>
                <th>Tipo</th>
                <th>Fecha Term.</th>
                <th>Años Trab.</th>
                <th>Vac. Pendientes</th>
                <th>Indemnización</th>
                <th>Total Neto</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {settlements.length === 0 ? (
                <tr><td colSpan="9" className="no-data">No hay liquidaciones registradas</td></tr>
              ) : (
                settlements.map(s => (
                  <tr key={s.id}>
                    <td><strong>{s.employeeName}</strong></td>
                    <td><span className="badge badge-info">{TERMINATION_TYPES[s.terminationType] || s.terminationType}</span></td>
                    <td>{formatDate(s.terminationDate)}</td>
                    <td>{s.workedYears} años {s.workedMonths} meses</td>
                    <td>{s.pendingVacationDays} días</td>
                    <td>{formatCurrency(s.severanceAmount)}</td>
                    <td><strong>{formatCurrency(s.netTotal)}</strong></td>
                    <td><span className={`badge ${STATUS_COLORS[s.status] || 'badge-secondary'}`}>{s.status}</span></td>
                    <td>
                      <button className="btn btn-sm btn-ghost" onClick={() => handleViewDetail(s.id)}>Ver</button>
                      {s.status === 'Calculada' && (
                        <button className="btn btn-sm btn-success" onClick={() => handleApprove(s.id)}>Aprobar</button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Modal Calcular */}
        {showCalculate && (
          <div className="modal-overlay" onClick={() => setShowCalculate(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Calcular Liquidación</h2>
                <button className="close-btn" onClick={() => setShowCalculate(false)}>&times;</button>
              </div>
              <div className="form-group">
                <label>Empleado *</label>
                <select value={form.employeeId} onChange={e => setForm({ ...form, employeeId: e.target.value })}>
                  <option value="">Seleccione empleado...</option>
                  {employees.map(emp => (
                    <option key={emp.id} value={emp.id}>{emp.fullName || `${emp.firstName} ${emp.lastName}`}</option>
                  ))}
                </select>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Tipo de Terminación *</label>
                  <select value={form.terminationType} onChange={e => setForm({ ...form, terminationType: e.target.value })}>
                    {Object.entries(TERMINATION_TYPES).map(([k, v]) => (
                      <option key={k} value={k}>{v}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label>Fecha de Terminación *</label>
                  <input type="date" value={form.terminationDate}
                    onChange={e => setForm({ ...form, terminationDate: e.target.value })} />
                </div>
              </div>
              <div className="form-group">
                <label>Deducciones adicionales</label>
                <div className="deduction-input-row">
                  <input type="text" placeholder="Descripción" value={newDeduction.description}
                    onChange={e => setNewDeduction({ ...newDeduction, description: e.target.value })} />
                  <input type="number" placeholder="Monto" value={newDeduction.amount}
                    onChange={e => setNewDeduction({ ...newDeduction, amount: e.target.value })} />
                  <button className="btn btn-ghost btn-sm" type="button" onClick={addDeduction}>Agregar</button>
                </div>
                {form.additionalDeductions.map((d, i) => (
                  <div key={i} className="deduction-tag">
                    <span>{d.description}: {formatCurrency(d.amount)}</span>
                    <button type="button" onClick={() => removeDeduction(i)}>×</button>
                  </div>
                ))}
              </div>
              <div className="form-group">
                <label>Notas</label>
                <textarea rows="2" value={form.notes} onChange={e => setForm({ ...form, notes: e.target.value })} />
              </div>
              {error && <div className="alert alert-error">{error}</div>}
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowCalculate(false)}>Cancelar</button>
                <button className="btn btn-primary" onClick={handleCalculate} disabled={calculating}>
                  {calculating ? 'Calculando...' : 'Calcular'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Modal Detalle */}
        {showDetail && selected && (
          <div className="modal-overlay" onClick={() => setShowDetail(false)}>
            <div className="modal" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Liquidación — {selected.employeeName}</h2>
                <button className="close-btn" onClick={() => setShowDetail(false)}>&times;</button>
              </div>
              <div className="info-box">
                <p><strong>Tipo:</strong> {TERMINATION_TYPES[selected.terminationType] || selected.terminationType}</p>
                <p><strong>Fecha contratación:</strong> {formatDate(selected.hireDate)} | <strong>Terminación:</strong> {formatDate(selected.terminationDate)}</p>
                <p><strong>Tiempo trabajado:</strong> {selected.workedYears} años, {selected.workedMonths} meses, {selected.workedDays} días</p>
                <p><strong>Último salario:</strong> {formatCurrency(selected.lastSalary)}</p>
              </div>
              <div className="settlement-breakdown">
                <div className="breakdown-row">
                  <span>Vacaciones pendientes ({selected.pendingVacationDays} días)</span>
                  <span>{formatCurrency(selected.vacationAmount)}</span>
                </div>
                <div className="breakdown-row">
  <span>Aguinaldo proporcional</span>
  <span>{formatCurrency(selected.proportionalBonus)}</span>
</div>
{selected.noticeAmount > 0 && (
  <div className="breakdown-row">
    <span>Preaviso (Art. 28 Cód. Trabajo)</span>
    <span>{formatCurrency(selected.noticeAmount)}</span>
  </div>
)}
<div className="breakdown-row">
  <span>Cesantía / Auxilio de cesantía</span>
  <span>{formatCurrency(selected.severanceAmount)}</span>
</div>
                {selected.deductions?.map(d => (
                  <div key={d.id} className="breakdown-row deduction">
                    <span>(-) {d.description}</span>
                    <span>({formatCurrency(d.amount)})</span>
                  </div>
                ))}
                <div className="breakdown-row gross">
                  <span>Total Bruto</span>
                  <span>{formatCurrency(selected.grossTotal)}</span>
                </div>
                <div className="breakdown-row total">
                  <span>Total Neto a Pagar</span>
                  <span>{formatCurrency(selected.netTotal)}</span>
                </div>
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowDetail(false)}>Cerrar</button>
                {selected.status === 'Calculada' && (
                  <button className="btn btn-success" onClick={() => handleApprove(selected.id)}>Aprobar</button>
                )}
                {selected.status === 'Aprobada' && (
                  <button className="btn btn-warning" onClick={() => handleMarkAsPaid(selected.id)}>
                    Marcar como Pagada
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

export default Settlements;
