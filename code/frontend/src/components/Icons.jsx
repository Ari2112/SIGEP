// Iconos de LÍNEA (stroke) compartidos en todo el sistema.
// Mismo estilo que los del menú lateral (Asistencia, Vacaciones, Horas Extra).
const stroke = {
  width: 22,
  height: 22,
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 1.8,
  strokeLinecap: 'round',
  strokeLinejoin: 'round',
};

export const Icon = {
  dashboard: (p) => (
    <svg {...stroke} {...p}><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/></svg>
  ),
  attendance: (p) => (
    <svg {...stroke} {...p}><circle cx="12" cy="12" r="9"/><path d="M8.5 12.5l2.5 2.5 4.5-5"/></svg>
  ),
  vacations: (p) => (
    <svg {...stroke} {...p}><path d="M2 22h20"/><path d="M6 18c4-6 9-9 14-9 0 0-1 6-7 8-3 1-5 1-7 1z"/><path d="M12 13c-2-3-2-7 1-10"/></svg>
  ),
  permissions: (p) => (
    <svg {...stroke} {...p}><rect x="5" y="4" width="14" height="17" rx="2"/><path d="M9 4h6v3H9z"/><path d="M8.5 11h7M8.5 15h7"/></svg>
  ),
  disabilities: (p) => (
    <svg {...stroke} {...p}><path d="M20.8 8.6a5 5 0 0 0-8.8-2.3A5 5 0 0 0 3.2 8.6c0 3.7 4.5 7 8.8 10.4 4.3-3.4 8.8-6.7 8.8-10.4z"/><path d="M12 9v5M9.5 11.5h5"/></svg>
  ),
  performance: (p) => (
    <svg {...stroke} {...p}><path d="M12 3.5l2.6 5.3 5.9.9-4.2 4.1 1 5.8L12 17l-5.3 2.6 1-5.8-4.2-4.1 5.9-.9z"/></svg>
  ),
  overtime: (p) => (
    <svg {...stroke} {...p}><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></svg>
  ),
  payroll: (p) => (
    <svg {...stroke} {...p}><rect x="2.5" y="5" width="19" height="14" rx="2"/><path d="M2.5 9.5h19"/><path d="M6 14.5h4"/></svg>
  ),
  bonus: (p) => (
    <svg {...stroke} {...p}><rect x="3" y="8" width="18" height="13" rx="1.5"/><path d="M3 12h18M12 8v13"/><path d="M12 8c-1.5-3-5-3-5-1s2.5 1 5 1zM12 8c1.5-3 5-3 5-1s-2.5 1-5 1z"/></svg>
  ),
  settlements: (p) => (
    <svg {...stroke} {...p}><path d="M7 3h7l5 5v13a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1z"/><path d="M14 3v5h5"/><path d="M9 13h6M9 17h6"/></svg>
  ),
  reports: (p) => (
    <svg {...stroke} {...p}><path d="M4 4v16h16"/><path d="M8 16v-4M12 16V8M16 16v-6"/></svg>
  ),
  employees: (p) => (
    <svg {...stroke} {...p}><circle cx="9" cy="8" r="3.2"/><path d="M3.5 19c0-3 2.5-5 5.5-5s5.5 2 5.5 5"/><path d="M16 6.5a3 3 0 0 1 0 5.8M17.5 19c0-2.2-1-4-2.7-4.7"/></svg>
  ),
};

export default Icon;