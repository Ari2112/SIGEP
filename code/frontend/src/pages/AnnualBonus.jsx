import { useState, useEffect } from 'react';
import { annualBonusAPI } from '../api/api';
import Layout from '../components/Layout';
import './AnnualBonus.css';

const STATUS_COLORS = {
  Borrador: 'badge-secondary',
  Calculado: 'badge-info',
  Aprobado: 'badge-success',
  Pagado: 'badge-warning',
  Anulado: 'badge-danger'
};

function AnnualBonus() {
  const [bonuses, setBonuses] = useState([]);
  const [selected, setSelected] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showCalculate, setShowCalculate] = useState(false);
  const [showDetail, setShowDetail] = useState(false);
  const [calculating, setCalculating] = useState(false);
  const [form, setForm] = useState({ year: new Date().getFullYear(), notes: '' });

  useEffect(() => { loadBonuses(); }, []);

  const loadBonuses = async () => {
    try {
      setLoading(true);
      const res = await annualBonusAPI.getAll();
      setBonuses(res.data);
    } catch (err) {
      setError('Error al cargar: ' + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const handleCalculate = async () => {
    try {
      setCalculating(true);
      setError('');
      await annualBonusAPI.calculate(form);
      setSuccess(`Aguinaldo ${form.year} calculado exitosamente`);
      setShowCalculate(false);
      loadBonuses();
    } catch (err) {
      setError(err.response?.data?.message || 'Error al calcular');
    } finally {
      setCalculating(false);
    }
  };

  const handleApprove = async (id) => {
    try {
      await annualBonusAPI.approve(id, '');
      setSuccess('Aguinaldo aprobado');
      loadBonuses();
      if (selected?.id === id) {
        const res = await annualBonusAPI.getById(id);
        setSelected(res.data);
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Error al aprobar');
    }
  };

  const handleRecalculate = async (id) => {
    try {
      await annualBonusAPI.recalculate(id);
      setSuccess('Aguinaldo recalculado');
      loadBonuses();
      if (selected?.id === id) {
        const res = await annualBonusAPI.getById(id);
        setSelected(res.data);
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Error al recalcular');
    }
  };

  const handleViewDetail = async (id) => {
    try {
      const res = await annualBonusAPI.getById(id);
      setSelected(res.data);
      setShowDetail(true);
    } catch (err) {
      setError('Error al cargar detalle');
    }
  };

  const formatCurrency = (v) => new Intl.NumberFormat('es-CR', { style: 'currency', currency: 'CRC', minimumFractionDigits: 0 }).format(v || 0);
  const formatDate = (d) => d ? new Date(d).toLocaleDateString('es-CR') : '-';

  if (loading) return <Layout><div className="loading">Cargando...</div></Layout>;

  return (
    <Layout>
      <div className="bonus-container">
        <div className="page-header">
          <div>
            <h1>Aguinaldo</h1>
            <p className="page-subtitle">Cálculo y gestión del aguinaldo anual</p>
          </div>
          <button className="btn btn-primary" onClick={() => { setShowCalculate(true); setError(''); }}>
            + Calcular Aguinaldo
          </button>
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {success && <div className="alert alert-success">{success}</div>}

        <div className="table-card">
          <table className="table">
            <thead>
              <tr>
                <th>Año</th>
                <th>Período</th>
                <th>Empleados</th>
                <th>Total</th>
                <th>Estado</th>
                <th>Calculado por</th>
                <th>Aprobado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {bonuses.length === 0 ? (
                <tr><td colSpan="8" className="no-data">No hay aguinaldos calculados</td></tr>
              ) : (
                bonuses.map(b => (
                  <tr key={b.id}>
                    <td><strong>{b.year}</strong></td>
                    <td>{formatDate(b.periodStartDate)} — {formatDate(b.periodEndDate)}</td>
                    <td>{b.totalEmployees}</td>
                    <td><strong>{formatCurrency(b.totalAmount)}</strong></td>
                    <td><span className={`badge ${STATUS_COLORS[b.status] || 'badge-secondary'}`}>{b.status}</span></td>
                    <td>{b.calculatedByName}</td>
                    <td>{b.approvedByName || '-'}</td>
                    <td>
                      <button className="btn btn-sm btn-ghost" onClick={() => handleViewDetail(b.id)}>Ver</button>
                      {b.status === 'Calculado' && (
                        <>
                          <button className="btn btn-sm btn-success" onClick={() => handleApprove(b.id)}>Aprobar</button>
                          <button className="btn btn-sm btn-warning" onClick={() => handleRecalculate(b.id)}>Recalcular</button>
                        </>
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
                <h2>Calcular Aguinaldo Anual</h2>
                <button className="close-btn" onClick={() => setShowCalculate(false)}>&times;</button>
              </div>
              <div className="form-group">
                <label>Año *</label>
                <input type="number" value={form.year}
                  onChange={e => setForm({ ...form, year: parseInt(e.target.value) })}
                  min="2020" max="2030" />
              </div>
              <div className="form-group">
                <label>Notas</label>
                <textarea rows="2" value={form.notes}
                  onChange={e => setForm({ ...form, notes: e.target.value })} />
              </div>
              <div className="alert alert-info">
                Se calculará el aguinaldo proporcional para todos los empleados activos del año {form.year}. El monto es 1 salario por año completo (proporcional a los meses trabajados).
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
            <div className="modal modal-lg" onClick={e => e.stopPropagation()}>
              <div className="modal-header">
                <h2>Aguinaldo {selected.year} — {formatCurrency(selected.totalAmount)}</h2>
                <button className="close-btn" onClick={() => setShowDetail(false)}>&times;</button>
              </div>
              <div className="info-box">
                <p><strong>Estado:</strong> <span className={`badge ${STATUS_COLORS[selected.status]}`}>{selected.status}</span></p>
                <p><strong>Total empleados:</strong> {selected.totalEmployees} | <strong>Total a pagar:</strong> {formatCurrency(selected.totalAmount)}</p>
                {selected.notes && <p><strong>Notas:</strong> {selected.notes}</p>}
              </div>
              <div className="table-card" style={{ marginTop: '16px' }}>
                <table className="table">
                  <thead>
                    <tr>
                      <th>Empleado</th>
                      <th>Puesto</th>
                      <th>Meses Trab.</th>
                      <th>Salario Promedio</th>
                      <th>Proporcional</th>
                      <th>Neto</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(selected.details || []).map(d => (
                      <tr key={d.id}>
                        <td>{d.employeeName}</td>
                        <td>{d.positionName || '-'}</td>
                        <td>{d.workedMonths} meses</td>
                        <td>{formatCurrency(d.averageSalary)}</td>
                        <td>{formatCurrency(d.proportionalAmount)}</td>
                        <td><strong>{formatCurrency(d.netAmount)}</strong></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <div className="modal-actions">
                <button className="btn btn-secondary" onClick={() => setShowDetail(false)}>Cerrar</button>
                {selected.status === 'Calculado' && (
                  <>
                    <button className="btn btn-warning" onClick={() => handleRecalculate(selected.id)}>Recalcular</button>
                    <button className="btn btn-success" onClick={() => handleApprove(selected.id)}>Aprobar</button>
                  </>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
}

export default AnnualBonus;
