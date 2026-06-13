export const ICONS = ['none', 'loading', 'filter-asc', 'filter-desc', 'filter', 'tag-empty', 'tag-filled', 'sent-empty', 'sent-filled'] as const;
export type IconType = typeof ICONS[number];