import { CATEGORY_COLORS } from '../../constants';

interface CategoryBadgeProps {
  category: string;
}

export default function CategoryBadge({ category }: CategoryBadgeProps) {
  const color = CATEGORY_COLORS[category] || '#6b7280';

  return (
    <span
      className="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium text-white"
      style={{ backgroundColor: color }}
    >
      {category}
    </span>
  );
}
