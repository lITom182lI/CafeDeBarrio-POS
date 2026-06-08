// Fuente: design-tokens/tokens.json (DTCG)
export const tokens = {
  color: {
    brand:      '#c2622a',
    brandHover: '#a85220',
    bg:         '#f5f6fa',
    card:       '#ffffff',
    text:       '#111827',
    muted:      '#6b7280',
    border:     '#e5e7eb',
    success:    '#22c55e',
    danger:     '#ef4444',
    warning:    '#f59e0b',
    sidebar:    '#1a1a1a',
  },
  dimension: {
    radius:   '10px',
    sidebarW: '220px',
  },
  chart: {
    palette: ['#c2622a', '#4a90d9', '#22c55e', '#9b59b6', '#f59e0b'] as const,
  },
} as const

export const brandPrimary  = tokens.color.brand
export const chartPalette  = tokens.chart.palette

export const semanticTokens = {
  color: {
    action:   { primary: tokens.color.brand,   primaryHover: tokens.color.brandHover },
    surface:  { page: tokens.color.bg,         card: tokens.color.card, sidebar: tokens.color.sidebar },
    text:     { default: tokens.color.text,    muted: tokens.color.muted },
    border:   { default: tokens.color.border },
    feedback: { success: tokens.color.success, danger: tokens.color.danger, warning: tokens.color.warning },
  },
} as const
