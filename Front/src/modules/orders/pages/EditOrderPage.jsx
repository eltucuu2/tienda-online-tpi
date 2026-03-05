import EditOrderForm from '../components/EditOrderForm';

const EditOrderPage = () => {
    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold mb-4 text-gray-800">Gestionar Orden</h1>
            <EditOrderForm />
        </div>
    );
};

export default EditOrderPage;