import {
  UtensilsCrossed, Car, Gamepad2, ShoppingBag, Heart, GraduationCap,
  Dumbbell, Receipt, Home, Coffee, Sparkles, MoreHorizontal,
  Wallet, Gift, Laptop, RotateCcw, TrendingUp,
} from 'lucide-react';
import { CATEGORY_COLORS } from '../../constants';
import { cn } from '../../utils/cn';

const ICON_MAP: Record<string, React.ElementType> = {
  Food: UtensilsCrossed,
  Transport: Car,
  Entertainment: Gamepad2,
  Shopping: ShoppingBag,
  Health: Heart,
  Education: GraduationCap,
  Sports: Dumbbell,
  Bills: Receipt,
  Rent: Home,
  Coffee: Coffee,
  PersonalCare: Sparkles,
  Salary: Wallet,
  Bonus: Gift,
  Freelance: Laptop,
  Gift: Gift,
  Refund: RotateCcw,
  Investment: TrendingUp,
  Other: MoreHorizontal,
};

const SIZES = {
  sm: { container: 'h-7 w-7', icon: 'h-3.5 w-3.5' },
  md: { container: 'h-9 w-9', icon: 'h-4.5 w-4.5' },
  lg: { container: 'h-11 w-11', icon: 'h-5.5 w-5.5' },
};

interface CategoryIconProps {
  category: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export default function CategoryIcon({ category, size = 'md', className }: CategoryIconProps) {
  const Icon = ICON_MAP[category] || MoreHorizontal;
  const color = CATEGORY_COLORS[category] || '#6b7280';
  const s = SIZES[size];

  return (
    <div
      className={cn('flex items-center justify-center rounded-full', s.container, className)}
      style={{ backgroundColor: `${color}20` }}
    >
      <Icon className={s.icon} style={{ color }} />
    </div>
  );
}
