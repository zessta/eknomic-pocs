import React, { useState } from 'react';
import { StyleSheet, View, Text, TextInput, Button, ScrollView, TouchableOpacity } from 'react-native';
import { Picker } from '@react-native-picker/picker';
import { RadioButton } from 'react-native-paper';
import PdfGenerator from './PdfGenerator';

const CRTScreen: React.FC = () => {
  const [formFields, setFormFields] = useState<any[]>([]);
  const [currentField, setCurrentField] = useState({ type: 'text', title: '', value: '', options: [] });
  const [editingIndex, setEditingIndex] = useState<number | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [newOption, setNewOption] = useState('');

  const handleAddField = () => {
    if (currentField.title && (currentField.type !== 'dropdown' || currentField.value)) {
      if (editingIndex !== null) {
        const updatedFields = [...formFields];
        updatedFields[editingIndex] = currentField;
        setFormFields(updatedFields);
        setEditingIndex(null);
      } else {
        setFormFields([...formFields, currentField]);
      }
      setCurrentField({ type: 'text', title: '', value: '', options: [] });
      setNewOption('');
    }
  };

  const handleEditField = (index: number) => {
    setCurrentField(formFields[index]);
    setEditingIndex(index);
  };

  const handleDeleteField = (index: number) => {
    const updatedFields = formFields.filter((_, i) => i !== index);
    setFormFields(updatedFields);
  };

  const handleSubmit = () => {
    formFields.length > 0 && setSubmitted(true);
  };

  const handleAddOption = () => {
    if (newOption) {
      setCurrentField((prevField) => ({
        ...prevField,
        options: [...prevField.options, newOption],
      }));
      setNewOption('');
    }
  };

  const renderFieldInput = () => {
    switch (currentField.type) {
      case 'text':
        return (
          <TextInput
            style={styles.input}
            placeholder="Enter value"
            value={currentField.value}
            onChangeText={(text) => setCurrentField({ ...currentField, value: text })}
          />
        );
      case 'radio':
        return (
          <>
            {currentField.options.map((option, index) => (
              <View key={index} style={styles.radioOption}>
                <RadioButton
                  value={option}
                  status={currentField.value === option ? 'checked' : 'unchecked'}
                  onPress={() => setCurrentField({ ...currentField, value: option })}
                />
                <Text>{option}</Text>
              </View>
            ))}
            <TextInput
              style={styles.input}
              placeholder="Add new option"
              value={newOption}
              onChangeText={setNewOption}
            />
            <Button title="Add Option" onPress={handleAddOption} />
          </>
        );
      case 'dropdown':
        return (
          <>
            {currentField.options.length > 0 && (
              <Picker
                selectedValue={currentField.value}
                onValueChange={(itemValue) => setCurrentField({ ...currentField, value: itemValue })}
                style={styles.picker}
              >
                {currentField.options.map((option, index) => (
                  <Picker.Item key={index} label={option} value={option} />
                ))}
              </Picker>
            )}
            <TextInput
              style={styles.input}
              placeholder="Add new option"
              value={newOption}
              onChangeText={setNewOption}
            />
            <Button title="Add Option" onPress={handleAddOption} />
          </>
        );
      case 'boolean':
        return (
          <View style={styles.booleanContainer}>
            <Text>True</Text>
            <RadioButton
              value="true"
              status={currentField.value === 'true' ? 'checked' : 'unchecked'}
              onPress={() => setCurrentField({ ...currentField, value: 'true' })}
            />
            <Text>False</Text>
            <RadioButton
              value="false"
              status={currentField.value === 'false' ? 'checked' : 'unchecked'}
              onPress={() => setCurrentField({ ...currentField, value: 'false' })}
            />
          </View>
        );
      default:
        return null;
    }
  };

  return (
    <View style={styles.container}>
      <ScrollView style={styles.formContainer}>
        <Text style={styles.label}>Field Title:</Text>
        <TextInput
          style={styles.input}
          value={currentField.title}
          onChangeText={(text) => setCurrentField({ ...currentField, title: text })}
        />
        <Text style={styles.label}>Field Type:</Text>
        <Picker
          selectedValue={currentField.type}
          onValueChange={(itemValue) => setCurrentField({ ...currentField, type: itemValue, value: '', options: [] })}
          style={styles.picker}
        >
          <Picker.Item label="Text" value="text" />
          <Picker.Item label="Radio" value="radio" />
          <Picker.Item label="Dropdown" value="dropdown" />
          <Picker.Item label="Boolean" value="boolean" />
        </Picker>
        {renderFieldInput()}
        <View style={{marginTop: 10}}>
        <Button  title={editingIndex !== null ? "Update Field" : "Add Field"} onPress={handleAddField} />
        </View>
      </ScrollView>
      <View style={styles.outputContainer}>
        <Text style={styles.outputTitle}>Added Fields:</Text>
        {formFields.map((field, index) => (
          <View key={index} style={styles.editFieldContainer}>
            <Text>{`${field.title}: ${field.value}`}</Text>
            <View style={styles.editButtonGroup}>
              <TouchableOpacity onPress={() => handleEditField(index)} style={styles.button}>
                <Text>Edit</Text>
              </TouchableOpacity>
              <TouchableOpacity onPress={() => handleDeleteField(index)} style={styles.button}>
                <Text>Delete</Text>
              </TouchableOpacity>
            </View>
          </View>
        ))}
        <Button title="Submit" onPress={handleSubmit} />
      </View>
      <View style={styles.outputContainer}>
        {submitted && formFields.length > 0 && (
          <View style={styles.submittedContainer}>
            <Text style={styles.outputTitle}>Submitted Fields:</Text>
            {formFields.map((field, index) => (
              <View key={index} style={styles.fieldContainer}>
                <Text style={styles.fieldText}>{`${field.title}: ${field.value}`}</Text>
              </View>
            ))}
          </View>
        )}
      </View>
      <PdfGenerator fields={formFields} />
      </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 20,
  },
  formContainer: {
    flex: 1,
  },
  outputContainer: {
    marginTop: 20,
  },
  label: {
    fontWeight: 'bold',
    marginTop: 10,
  },
  input: {
    borderColor: '#ccc',
    borderWidth: 1,
    borderRadius: 4,
    padding: 10,
    marginVertical: 5,
  },
  radioOption: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  booleanContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginVertical: 10,
  },
  outputTitle: {
    fontWeight: 'bold',
    marginBottom: 10,
  },
  submittedContainer: {
    padding: 10,
    borderColor: '#ccc',
    borderWidth: 1,
    borderRadius: 4,
    marginTop: 10,
  },
  fieldContainer: {
    marginVertical: 5,
  },
  editButtonGroup: {
    flexDirection: 'row',
  },
  editFieldContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginVertical: 5,
  },
  fieldText: {
    fontSize: 16,
  },
  button: {
    marginLeft: 10,
    backgroundColor: '#ccc',
    padding: 5,
    borderRadius: 4,
  },
  picker: {
    height: 50,
    width: '100%',
    marginVertical: 10,
  },
});

export default CRTScreen;
