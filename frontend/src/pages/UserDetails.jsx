import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { getUser } from '../services/api'

function UserDetails() {
  const navigate = useNavigate()
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    const fetchUser = async () => {
      try {
        const response = await getUser()
        setUser(response.data)
      } catch (err) {
        if (err.response?.status === 401) {
          localStorage.removeItem('token')
          navigate('/login')
        } else {
          setError('Failed to load user details.')
        }
      } finally {
        setLoading(false)
      }
    }
    fetchUser()
  }, [navigate])

  const handleLogout = () => {
    localStorage.removeItem('token')
    navigate('/login')
  }

  if (loading) {
    return (
      <div className="container">
        <div className="card">
          <p style={{ textAlign: 'center' }}>Loading...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="container">
        <div className="card">
          <div className="alert alert-error">{error}</div>
          <button className="btn btn-danger" onClick={handleLogout}>Logout</button>
        </div>
      </div>
    )
  }

  return (
    <div className="container">
      <div className="card">
        <div className="avatar">
          {user?.firstName?.[0]}{user?.lastName?.[0]}
        </div>
        <h1>User Profile</h1>
        <hr className="divider" />
        <div className="user-info">
          <div className="label">First Name</div>
          <div className="value">{user?.firstName}</div>
        </div>
        <div className="user-info">
          <div className="label">Last Name</div>
          <div className="value">{user?.lastName}</div>
        </div>
        <div className="user-info">
          <div className="label">Email</div>
          <div className="value">{user?.email}</div>
        </div>
        <button className="btn btn-danger" onClick={handleLogout}>Logout</button>
      </div>
    </div>
  )
}

export default UserDetails
