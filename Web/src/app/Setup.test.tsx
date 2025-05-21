import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { BrowserRouter } from 'react-router-dom'; // To provide context for useNavigate
import SetupPage from './Setup'; // Adjust path as necessary

// Mock urql
const mockCreateUserMutation = vi.fn();
const mockSignInMutation = vi.fn();
vi.mock('urql', async (importOriginal) => {
    const actual = await importOriginal() as typeof import('urql');
    return {
        ...actual,
        useMutation: (query: any) => {
            if (query.definitions[0].name.value === 'CreateUser') {
                return [{}, mockCreateUserMutation];
            }
            if (query.definitions[0].name.value === 'SignIn') {
                return [{}, mockSignInMutation];
            }
            return [{}, vi.fn()];
        },
    };
});

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async (importOriginal) => {
    const actual = await importOriginal() as typeof import('react-router-dom');
    return {
        ...actual,
        useNavigate: () => mockNavigate,
    };
});

// Mock window.location.reload
Object.defineProperty(window, 'location', {
    configurable: true,
    value: { reload: vi.fn() },
});


describe('SetupPage', () => {
    beforeEach(() => {
        // Clear mocks before each test
        mockCreateUserMutation.mockClear();
        mockSignInMutation.mockClear();
        mockNavigate.mockClear();
        (window.location.reload as ReturnType<typeof vi.fn>).mockClear();
    });

    test('renders correctly with username, password inputs, and submit button', () => {
        render(
            <BrowserRouter>
                <SetupPage />
            </BrowserRouter>
        );

        expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /create admin user/i })).toBeInTheDocument();
    });

    test('typing into inputs updates their values', () => {
        render(
            <BrowserRouter>
                <SetupPage />
            </BrowserRouter>
        );

        const usernameInput = screen.getByLabelText(/username/i) as HTMLInputElement;
        const passwordInput = screen.getByLabelText(/password/i) as HTMLInputElement;

        fireEvent.change(usernameInput, { target: { value: 'testuser' } });
        fireEvent.change(passwordInput, { target: { value: 'password123' } });

        expect(usernameInput.value).toBe('testuser');
        expect(passwordInput.value).toBe('password123');
    });

    test('submitting the form calls createUser, then signIn, then navigates and reloads', async () => {
        mockCreateUserMutation.mockResolvedValueOnce({
            data: { createUser: { __typename: 'CreateUserSuccess', user: { id: '1', username: 'testuser' } } },
            error: null,
        });
        mockSignInMutation.mockResolvedValueOnce({
            data: { signIn: { __typename: 'SignInSuccess', user: { id: '1', username: 'testuser' } } },
            error: null,
        });

        render(
            <BrowserRouter>
                <SetupPage />
            </BrowserRouter>
        );

        fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'testuser' } });
        fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
        fireEvent.click(screen.getByRole('button', { name: /create admin user/i }));

        await waitFor(() => {
            expect(mockCreateUserMutation).toHaveBeenCalledWith({
                username: 'testuser',
                password: 'password123',
            });
        });
        await waitFor(() => {
            expect(mockSignInMutation).toHaveBeenCalledWith({
                username: 'testuser',
                password: 'password123',
            });
        });
        await waitFor(() => {
            expect(mockNavigate).toHaveBeenCalledWith('/', { replace: true });
        });
        await waitFor(() => {
            expect(window.location.reload).toHaveBeenCalled();
        });
    });

    test('displays error message if createUser mutation fails', async () => {
        mockCreateUserMutation.mockResolvedValueOnce({
            data: null,
            error: { message: 'Create user failed' },
        });
        
        render(
            <BrowserRouter>
                <SetupPage />
            </BrowserRouter>
        );

        fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'testuser' } });
        fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
        fireEvent.click(screen.getByRole('button', { name: /create admin user/i }));

        await waitFor(() => {
            expect(screen.getByText(/create user failed/i)).toBeInTheDocument();
        });
        expect(mockSignInMutation).not.toHaveBeenCalled();
        expect(mockNavigate).not.toHaveBeenCalled();
    });

    test('displays error message if createUser returns CreateUserError', async () => {
        mockCreateUserMutation.mockResolvedValueOnce({
            data: { createUser: { __typename: 'CreateUserError', message: 'Username already exists' } },
            error: null,
        });

        render(
            <BrowserRouter>
                <SetupPage />
            </BrowserRouter>
        );
        
        fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'testuser' } });
        fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
        fireEvent.click(screen.getByRole('button', { name: /create admin user/i }));

        await waitFor(() => {
            expect(screen.getByText(/username already exists/i)).toBeInTheDocument();
        });
    });


    test('displays error message if signIn mutation fails', async () => {
        mockCreateUserMutation.mockResolvedValueOnce({
            data: { createUser: { __typename: 'CreateUserSuccess', user: { id: '1', username: 'testuser' } } },
            error: null,
        });
        mockSignInMutation.mockResolvedValueOnce({
            data: null,
            error: { message: 'Sign in failed' },
        });

        render(
            <BrowserRouter>
                <SetupPage />
            </BrowserRouter>
        );

        fireEvent.change(screen.getByLabelText(/username/i), { target: { value: 'testuser' } });
        fireEvent.change(screen.getByLabelText(/password/i), { target: { value: 'password123' } });
        fireEvent.click(screen.getByRole('button', { name: /create admin user/i }));

        await waitFor(() => {
            expect(screen.getByText(/sign in failed/i)).toBeInTheDocument();
        });
        expect(mockNavigate).not.toHaveBeenCalled();
    });
});
