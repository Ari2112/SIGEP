import { useState, useEffect } from 'react';
import Layout from '../components/Layout';
import { employeeAPI } from '../api/api';
import './Employees.css';

const Employees = () => {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadEmployees();
  }, []);

  const loadEmployees = async () => {
    try {
      setLoading(true);
      const response = await employeeAPI.getAll();
      setEmployees(response.data);
      setError('');
    } catch (err) {
      setError('Error al cargar los empleados');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-CR', {
      style: 'currency',
      currency: 'CRC',
    }).format(amount);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('es-CR');
  };

  return (
    <Layout>
      <div className="employees">
        <div className="page-header">
          <h1>Gestión de Empleados</h1>
        </div>

        {error && (
          <div className="alert alert-error">
            {error}
          </div>
        )}

        {loading ? (
          <div className="card">
            <p>Cargando empleados...</p>
          </div>
        ) : (
          <div className="card">
            <div className="table-responsive">
              <table>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Nombre Completo</th>
                    <th>Identificación</th>
                    <th>Email</th>
                    <th>Puesto</th>
                    <th>Salario Base</th>
                    <th>Fecha Ingreso</th>
                    <th>Estado</th>
                  </tr>
                </thead>
                <tbody>
                  {employees.length === 0 ? (
                    <tr>
                      <td colSpan="8" style={{ textAlign: 'center' }}>
                        No hay empleados registrados
                      </td>
                    </tr>
                  ) : (
                    employees.map((employee) => (
                      <tr key={employee.id}>
                        <td>{employee.id}</td>
                        <td>{employee.fullName}</td>
                        <td>{employee.identificationNumber}</td>
                        <td>{employee.email}</td>
                        <td>{employee.positionName || '-'}</td>
                        <td>{formatCurrency(employee.baseSalary)}</td>
                        <td>{formatDate(employee.hireDate)}</td>
                        <td>
                          <span className={`status-badge status-${employee.status.toLowerCase()}`}>
                            {employee.status}
                          </span>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default Employees;
