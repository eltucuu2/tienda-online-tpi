import CreateProductForm from '../components/CreateProductForm';

function CreateProductPage() {
  return (
    <div className="p-4">
        <h1 className="text-2xl font-bold mb-4 text-gray-800">Alta de Producto</h1>
        <CreateProductForm />
    </div>
  );
}

export default CreateProductPage;