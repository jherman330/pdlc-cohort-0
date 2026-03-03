import { render, screen } from '../test-utils/render';
import HomePage from './homePage';

describe('HomePage', () => {
  it('renders the main heading', () => {
    render(<HomePage />);
    expect(screen.getByText(/Add your own application code/i)).toBeInTheDocument();
  });

  it('renders the scaffold description', () => {
    render(<HomePage />);
    expect(screen.getByText(/minimal scaffold with React/i)).toBeInTheDocument();
  });
});
