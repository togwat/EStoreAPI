import { Link } from 'react-router-dom';
import './MenuButton.css';

function MenuButton() {
      return (
          <Link to="/" className="back-button">
              <button>Back</button>
        </Link>
    );
}

export default MenuButton;